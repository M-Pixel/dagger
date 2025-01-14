using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dagger.Dev;
using static Global;
using static Alias;

public static class Targets
{
	/// <summary>A container with the Dotnet SDK (not the Dotnet Dagger SDK, the Microsoft Dotnet SDK).</summary>
	public static Container ToolsContainer() => DotnetSDKProject.ToolsContainer();

	/// <summary>The Client library project (does not include parent solution).</summary>
	/// <param name="source">The source code for the project (should contain "./Client.csporj").</param>
	public static DotnetSDKProject Project([DirectoryFromContext] Directory source) => new
	(
		ToolsContainer()
			.WithDirectory(".", source, include: ["*.csproj"], owner: UID)
			.WithExec(["dotnet", "restore", "Client.csproj"])
			.WithDirectory(".", source, include: ["*.cs"], owner: UID),
		source.File(DotnetSDKProject.ICON_PATH)
	);

	/// <summary>Runs automated tests.</summary>
	/// <param name="source">The source code for the project (should contain "./Client.csporj").</param>
	public static TestedDotnetSDKProject Test([DirectoryFromContext] Directory source) => Project(source).Test();

	/// <summary>Produces NuGet packages.</summary>
	/// <param name="source">The source code for the project (should contain "./Client.csporj").</param>
	public static Directory Package([DirectoryFromContext] Directory source) => Project(source).Package();

	/// <summary>Publishes the Client library to NuGet</summary>
	/// <param name="source">The source code for the project (should contain "./Client.csporj").</param>
	public static Task Publish([DirectoryFromContext] Directory source, Secret nugetApiKey) =>
		Test(source).Publish(nugetApiKey);

	/// <summary>
	///		Publishes the Client library to a local NuGet server, and returns that server as a service.  Run this and
	///		&lt;add key="local" value="http://localhost:8080/v3/index.json" allowInsecureConnections="true" /&gt; to
	///		<c>nuget.config</c> to test installing the latest local iteration of the Client library into other projects.
	/// </summary>
	/// <param name="source">The source code for the project (should contain "./Client.csporj").</param>
	public static Task<Service> Serve([DirectoryFromContext] Directory source) => Project(source).Serve();
}

public record DotnetSDKProject(Container Container, File IconFile)
{
	internal const string ICON_PATH = "dagger-icon.png";


	internal static Container ToolsContainer() => DAG
		.Container()
		.From("mcr.microsoft.com/dotnet/sdk:8.0-alpine3.20")
		.WithUser(UID)
		.WithEnvVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
		.WithWorkdir("/home/app/source")
		.WithDirectory(".", DAG.GetDirectory(), owner: UID);

	public TestedDotnetSDKProject Test() => throw new NotImplementedException();

	public Directory Package() => DAG.GetDirectory().WithDirectory
	(
		".",
		Container
			.WithFile(ICON_PATH, IconFile, owner: UID)
			.WithExec(["dotnet", "pack", "--no-restore", "--include-symbols", "Client.csproj"])
			.Directory("bin/Release"),
		include: ["*.nupkg"]
	);

	public async Task Publish(Service toService, int? port)
	{
		string portString = (port ?? await (await toService.Ports()).First().SubPort()).ToString();
		await
			(
				this with
				{
					Container = Container.WithServiceBinding("nuget", toService)
				}
			)
			.Publish(new Uri("http://nuget:" + portString));
	}

	internal async Task Publish
	(
		Uri nugetServer,
		Task<IEnumerable<string>>? additionalArgumentsTask = null
	)
	{
		var packages = Package();
		await Task.WhenAll
		(
			ImmutableArrayExtensions.Select<string, Task>
			(
				await packages.Entries(),
				packageName => Task.Run
				(
					async () =>
					{
						IEnumerable<string> additional = additionalArgumentsTask != null
							? await additionalArgumentsTask
							: [];
						await Targets.ToolsContainer()
							.WithMountedDirectory(".", packages)
							.WithExec
							(
								["dotnet", "nuget", "push", packageName, ..additional, $"--source={nugetServer}"]
							)
							.Sync();
					}
				)
			)
		);
	}

	public async Task<Service> Serve() => (await new NugetServer().WithPublished(this)).AsService();
}

public record TestedDotnetSDKProject(Container BuildEnvironment, File IconFile) // TODO: .With(...), make sure it works with records
{
	public Directory Package() => AsProject.Package();

	public Task Publish(Secret nugetApiKey) => AsProject.Publish
	(
		new Uri("https://api.nuget.org/v3/index.json"),
		nugetApiKey.Plaintext().ContinueWith<IEnumerable<string>>(apiKeyTask => [$"--api-key={apiKeyTask.Result}"])
	);

	public Task<Service> Serve() => AsProject.Serve();

	private DotnetSDKProject AsProject => new(BuildEnvironment, IconFile);
}

public record NugetServer
{
	public Container ReadyToServe { get; init; }


	public NugetServer()
	{
		ReadyToServe = DotnetSDKProject.ToolsContainer()
			.WithMountedDirectory
			(
				"/mnt/bagetter",
				DotnetSDKProject.ToolsContainer()
					.WithDirectory("/srv/bagetter", DAG.GetDirectory(), owner: UID)
					.WithMountedFile
					(
						"/mnt/bagetter.zip",
						DAG.Http("https://github.com/bagetter/BaGetter/releases/download/v1.5.1/bagetter-1.5.1.zip"),
						owner: UID
					)
					.WithExec(["unzip", "/mnt/bagetter.zip", "-d", "/srv/bagetter"])
					.Directory("/srv/bagetter"),
				owner: UID
			)
			.WithWorkdir("/mnt/bagetter")
			.WithDirectory("/var/bagetter", DAG.GetDirectory(), owner: UID)
			.WithEnvVariable("AllowPackageOverwrites", "true")
			.WithEnvVariable("Database__ConnectionString", "Data Source=/var/bagetter/bagetter.db")
			.WithEnvVariable("Storage__Path", "/var/bagetter/packages")
			.WithEnvVariable("AllowPackageOverwrites", "true")
			.WithEntrypoint(["dotnet"])
			.WithExposedPort(8080, NetworkProtocol.TCP)
			.WithDefaultArgs(["/mnt/bagetter/BaGetter.dll"]);
	}

	public Service AsService() => ReadyToServe.AsService(useEntrypoint: true);

	public async Task<NugetServer> WithPublished(DotnetSDKProject project)
	{
		var cache = DAG.CacheVolume(Guid.NewGuid().ToString());
		Service service = ReadyToServe
			.WithMountedCache("/var/bagetter", cache, owner: UID)
			.AsService(useEntrypoint: true);
		await project.Publish(service, 8080);
		await service.Stop();
		return this with
		{
			ReadyToServe = ReadyToServe
				.WithMountedCache("/mnt/state", cache, owner: UID)
				.WithExec(["sh", "-c", "cp -r /mnt/state/* /var/bagetter/"])
				.WithoutMount("/mnt/state")
		};
	}
}
