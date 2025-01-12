using System.Collections.Immutable;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Dagger.Primer;

/// <summary>
///		A teeny tiny NuGet client that uses <c>.deps.json</c> instead of <c>.csproj</c>.  Installs packages to /nuget
///		and creates symlinks in /module-deps, so that runtime assembly resolution can be baked ahead of time.
/// </summary>
static class NuGetClient
{
	private static readonly string _nugetPackagesPath = "/nuget";
	private static string _linksPath = "/module-deps/";

	static NuGetClient()
	{
		if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
		{
			Console.WriteLine("NOT IN CONTAINER");
			_nugetPackagesPath = Environment.CurrentDirectory + _nugetPackagesPath;
			Directory.CreateDirectory(_nugetPackagesPath);
			_linksPath = Environment.CurrentDirectory + _linksPath;
			Directory.CreateDirectory(_linksPath);
		}
	}

	public static async Task Restore(string projectDirectoryPath, string depsFilePath)
	{
		if (Environment.GetEnvironmentVariable("Dagger:Primer:DirectDeps") != null)
			_linksPath = Path.GetDirectoryName(depsFilePath)! + '/';

		ISettings settings = Settings.LoadDefaultSettings(projectDirectoryPath);
		PackageSourceProvider packageSourceProvider = new
		(
			settings,
			[
				new FeedTypePackageSource(NuGetConstants.V3FeedUrl, FeedType.HttpV3)
				{
					IsOfficial = true,
					ProtocolVersion = 3
				}
			]
		);
		ImmutableArray<Source> sources =
		[
			..packageSourceProvider.LoadPackageSources()
				.Where(source => source.IsEnabled)
				.Select
				(
					source =>
					{
						SourceRepository repository = Repository.Factory.GetCoreV3(source);
						return new Source
						(
							repository.GetResourceAsync<FindPackageByIdResource>(),
							repository.GetResourceAsync<DownloadResource>()
						);
					}
				)
		];


		DependencyContext dependencyContext;
		using (DependencyContextJsonReader reader = new())
		await using (FileStream fileStream = new(depsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			dependencyContext = reader.Read(fileStream);
		// TODO: Test that this is correct for multi-platform packages.
		RuntimeFallbacks? runtimeFallbacks = dependencyContext.RuntimeGraph.FirstOrDefault();
		ImmutableArray<string> runtimes = runtimeFallbacks == null
			? [""]
			: [..runtimeFallbacks.Fallbacks.Prepend(runtimeFallbacks.Runtime).Append("")!];
		PackageCompilationAssemblyResolver packageResolver = new(_nugetPackagesPath);

		SourceCacheContext sourceCacheContext = new();
		PackageDownloadContext downloadContext = new(sourceCacheContext);
		// TODO: Possible to avoid retaining nupkg, pdb, and docs?  Possible to extract while streaming, no nupkg on disk at all?

		await Task.WhenAll
		(
			dependencyContext.RuntimeLibraries.Select(library => ((Func<Task>)(async () =>
			{
				if (!NuGetVersion.TryParse(library.Version, out NuGetVersion? version))
				{
					await Console.Error.WriteLineAsync($"Couldn't parse version from \"{library.Version}\".");
					return;
				}

				PackageIdentity identity = new(library.Name, version);
				foreach (Source source in sources)
				{
					FindPackageByIdResource packageFinder = await source.FindPackageById;
					bool doesPackageExist = await packageFinder
						.DoesPackageExistAsync
						(
							identity.Id, identity.Version, sourceCacheContext, NullLogger.Instance, CancellationToken.None
						);
					if (!doesPackageExist)
						return;

					DownloadResource downloader = await source.Download;
					DownloadResourceResult? result = await downloader.GetDownloadResourceResultAsync
					(
						identity,
						downloadContext,
						_nugetPackagesPath,
						NullLogger.Instance,
						CancellationToken.None
					);

					if (result == null || result.Status == DownloadResourceResultStatus.NotFound)
					{
						Console.WriteLine($"{library.Name} not found in {source}");
						continue;
					}

					CompilationLibrary asCompilationLibrary = new
					(
						type: library.Type,
						name: library.Name,
						version: library.Version,
						hash: library.Hash,
						assemblies: runtimes
							.Select
							(
								runtimeIdentifier => library.RuntimeAssemblyGroups
									.FirstOrDefault(group => group.Runtime == runtimeIdentifier)
							)
							.First(group => group != null)!
							.RuntimeFiles
							.Select(file => file.Path),
						dependencies: library.Dependencies,
						serviceable: library.Serviceable,
						path: library.Path,
						hashPath: library.HashPath
					);
					List<string> results = new();
					packageResolver.TryResolveAssemblyPaths(asCompilationLibrary, /* out */ results);
					foreach (string dependencyPath in results)
					{
						string leaf = Path.GetFileName(dependencyPath);
						File.CreateSymbolicLink(_linksPath + leaf, dependencyPath);
						Console.WriteLine(_linksPath + leaf + '→' + dependencyPath);
					}
				}
			}))())
		);
	}
}

readonly record struct Source(Task<FindPackageByIdResource> FindPackageById, Task<DownloadResource> Download);
