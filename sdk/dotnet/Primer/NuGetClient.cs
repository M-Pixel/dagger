using System.Collections.Immutable;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyModel;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Dagger.Primer;

/// <summary>
///		A teeny tiny NuGet client that uses <c>.deps.json</c> instead of <c>.csproj</c>.  Installs packages to
///		<c>/Dependencies</c>, effectively compressing the download extract and copy operations employed by
///		<c>dotnet restore</c> and <c>dotnet build</c> into a single step.  Alternatively, installs a package <em>as</em>
///		a module.
/// </summary>
/// <remarks>
///		<p>The Thunk program is written in coordination with this behavior, expecting to find module dependencies in the
///		<c>/Dependencies</c> folder instead of just in the same directory as the executing assembly, as mounted modules
///		can't have files added to their directory (mounts are read-only).  However, when Primer itself installs the
///		module, instead of priming the container for a mounted module, dependencies are placed in the same folder.  This
///		is not just an optimization, but enables the use of Primer as a means of retrieving pre-compiled components of
///		the Dagger Dotnet SDK (Thunk, Code Generator), rather than compiling them from source and therefore requiring
///		the full 700+ MiB Dotnet SDK as a prerequisite for simply invoking other authors' Dotnet modules, and instead of
///		distributing assembly files through Git repositories.</p>
/// </remarks>
class NuGetClient
{
	private static readonly string _dependencyInstallPath = "/Dependencies/";
	private static readonly string _moduleInstallRoot = "/Module/";
	private static readonly string _referenceAssembliesPath = "/Reference/";

	private readonly ImmutableArray<Task<FindPackageByIdResource>> _sources;
	private readonly SourceCacheContext _sourceCacheContext = new();

	static NuGetClient()
	{
		// Make it possible to use outside of a container, for development testing purposes.
		if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
		{
			Console.WriteLine("NOT IN CONTAINER");

			_dependencyInstallPath = Environment.CurrentDirectory + _dependencyInstallPath;
			Directory.CreateDirectory(_dependencyInstallPath);

			_moduleInstallRoot = Environment.CurrentDirectory + "/TempModule/";

			_referenceAssembliesPath = Environment.CurrentDirectory + _referenceAssembliesPath;
			Directory.CreateDirectory(_referenceAssembliesPath);
		}
	}

	/// <param name="projectDirectoryPath">
	///		If a mounted module has a <c>NuGet.config</c> file, it should be respected by this client.  To do so, I need
	///		to know where the module is.
	/// </param>
	public NuGetClient(string? projectDirectoryPath = null)
	{
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
		_sources =
		[
			..packageSourceProvider.LoadPackageSources()
				.Where(source => source.IsEnabled)
				.Select
				(
					source =>
					{
						SourceRepository repository = Repository.Factory.GetCoreV3(source);
						return repository.GetResourceAsync<FindPackageByIdResource>();
					}
				)
		];
	}


	/// <summary>Install a Dagger Module (or SDK program) from NuGet.</summary>
	public async Task<string> InstallModule(string packageName, NuGetVersion packageVersion)
	{
		string moduleName = packageName[(packageName.LastIndexOf('.') + 1)..];
		List<Task> fileWriteTasks = new();

		ZipArchive nupkg = await GetPackage(packageName, packageVersion)
			?? throw new Exception($"Couldn't find package {packageName} version {packageVersion}");
		// TODO: Support multi-assembly packages (see which targets package contains, use frameworkreducer against executing runtime)
		Task? settingsExtractTask = null;
		Stream? dependenciesJson = null;
		string? moduleAssemblyPath = null;
		string installPath = _moduleInstallRoot + moduleName + '/';
		foreach (ZipArchiveEntry packageFile in nupkg.Entries)
		{
			// TODO: Extract other files, too, maintain subdirectory structure (allow arbitrary asset inclusion, same as git repo can have context above source)
			if (packageFile.Name.EndsWith(".dll"))
			{
				fileWriteTasks.Add(Extract(packageFile, installPath));
				// Might be a single-dll package with no dependencies.
				moduleAssemblyPath ??= packageFile.FullName;
			}
			else if (packageFile.Name.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase))
			{
				// If a deps file exists, it will be paired with an assembly of the same name, which will be our primary
				// assembly (the package name might be different, e.g. have an additional org prefix).
				moduleAssemblyPath = packageFile.FullName[..^9] + "dll";
				dependenciesJson = packageFile.Open();
			}
			else if
			(
				string.Equals(packageFile.Name, Settings.DefaultSettingsFileName, StringComparison.OrdinalIgnoreCase)
			)
				settingsExtractTask = Extract(packageFile, installPath);
		}

		if (dependenciesJson != null)
		{
			if (settingsExtractTask == null)
				await RestoreModule(dependenciesJson, installPath, installPath);
			else
			{
				await settingsExtractTask;
				await new NuGetClient(installPath).RestoreModule(dependenciesJson, installPath, installPath);
			}
		}

		foreach (Task fileWriteTask in fileWriteTasks)
			await fileWriteTask;

		return installPath + moduleAssemblyPath;
	}

	/// <summary>Restore the NuGet dependencies of a Dagger Module that was mounted from a context directory.</summary>
	public async Task RestoreModule(Stream depsFileStream, string assemblyDirectoryPath, string? installPath = null)
	{
		installPath ??= _dependencyInstallPath;
		Console.WriteLine($"Restoring to {installPath} for assembly in {assemblyDirectoryPath}");

		// Dependency Context parses .deps.json
		DependencyContext dependencyContext;
		using (DependencyContextJsonReader reader = new())
			dependencyContext = reader.Read(depsFileStream);
		ValueTask disposeTask = depsFileStream.DisposeAsync();

		// If the module was published with support for multiple target frameworks, determine which of them is most
		// appropriate for the current runtime.
		ImmutableArray<string> runtimeTargetPreferenceList;
		IReadOnlyList<RuntimeFallbacks> runtimeGraph = dependencyContext.RuntimeGraph;
		if (runtimeGraph.Count == 0)
			runtimeTargetPreferenceList = [""];
		else
		{
			// TODO: Figure out how to select the correct RuntimeGraph when there are more than one.
			RuntimeFallbacks runtimeFallbacks = dependencyContext.RuntimeGraph[0];
			runtimeTargetPreferenceList = [..runtimeFallbacks.Fallbacks.Prepend(runtimeFallbacks.Runtime).Append("")!];
		}

		IEnumerable<RuntimeLibrary> runtimeDependencies =
			dependencyContext.RuntimeLibraries.Where(library => library.RuntimeAssemblyGroups.Count > 0);
		await Task.WhenAll
		(
			runtimeDependencies.Select(library => ((Func<Task>)(async () =>
			{
				// If the package contains multiple target frameworks figure out which one to extract
				RuntimeAssetGroup filesGroup = runtimeTargetPreferenceList
					.Select
					(
						runtimeIdentifier => library.RuntimeAssemblyGroups
							.FirstOrDefault(group => group.Runtime == runtimeIdentifier)
					)
					.FirstOrDefault(group => group != null)
					?? throw new Exception($"{library.Name} assembly groups [{string.Join(',', library.RuntimeAssemblyGroups.Select(group => group.Runtime))}] no match for preference list [{string.Join(',', runtimeTargetPreferenceList)}]");
				// Filter for files that don't already exist
				List<string> filesToExtract = new(filesGroup.AssetPaths.Count);
				foreach (string filePath in filesGroup.AssetPaths)
					if (!File.Exists(assemblyDirectoryPath + Path.GetFileName(filePath)))
						filesToExtract.Add(filePath);
				if (filesToExtract.Count == 0)
					return;

				if (!NuGetVersion.TryParse(library.Version, out var version))
				{
					await Console.Error.WriteLineAsync($"Couldn't parse version from \"{library.Version}\".");
					return;
				}

				ZipArchive? nupkg = await GetPackage(library.Name, version);
				if (nupkg == null)
				{
					Console.Error.WriteLine($"Couldn't find package {library.Name} version {version}");
					return;
				}

				// If module is in the container's filesystem, I receive a directInstallPath and put the
				// dependencies in the same folder as the assembly that needs them.  If module is mounted
				await Task.WhenAll
				(
					filesToExtract.Select
					(
						packageFilePath =>
						{
							ZipArchiveEntry? entry = nupkg.GetEntry(packageFilePath);
							if (entry == null)
								return Task.CompletedTask;
							return Extract(entry, installPath);
						}
					)
				);
			}))())
		);
		await disposeTask;
	}

	public async Task InstallReferenceAssemblies()
	{
		NuGetVersion version = NuGetVersion.Parse(RuntimeInformation.FrameworkDescription[5..]);
		using ZipArchive nupkg = await GetPackage("Microsoft.NETCore.App.Ref", version)
			?? throw new Exception($"Couldn't find reference assemblies package for version {version}!");

		foreach (ZipArchiveEntry packageFile in nupkg.Entries)
			if (packageFile.FullName.StartsWith("ref/") && packageFile.Name.EndsWith(".dll"))
				await Extract(packageFile, _referenceAssembliesPath);
	}


	private static async Task Extract(ZipArchiveEntry entry, string destinationDirectoryPath)
	{
		FileStreamOptions options = new()
		{
			Access = FileAccess.Write,
			Mode = FileMode.CreateNew,
			Share = FileShare.Read,
			PreallocationSize = entry.Length,
#pragma warning disable CA1416 // because Dagger doesn't support Windows containers
			UnixCreateMode = UnixFileMode.UserRead,
#pragma warning restore CA1416
			Options = FileOptions.Asynchronous | FileOptions.SequentialScan
		};
		await using FileStream outStream = new(destinationDirectoryPath + entry.Name, options);
		await using Stream inStream = entry.Open();
		await inStream.CopyToAsync(outStream);
	}

	private async Task<ZipArchive?> GetPackage(string packageName, NuGetVersion packageVersion)
	{
		MemoryStream nupkgFile = new();
		foreach (Task<FindPackageByIdResource> sourceTask in _sources)
		{
			FindPackageByIdResource source = await sourceTask;
			Task<bool> copyToStreamTask = source.CopyNupkgToStreamAsync
			(
				packageName,
				packageVersion,
				nupkgFile,
				_sourceCacheContext,
				NullLogger.Instance,
				CancellationToken.None
			);
			if (!await copyToStreamTask)
				continue;

			return new ZipArchive(nupkgFile, ZipArchiveMode.Read, leaveOpen: false);
		}

		return null;
	}
}
