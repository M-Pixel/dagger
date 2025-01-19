using System;
using System.Threading.Tasks;

namespace Dagger.DotnetCLI;
using static Alias;
using static Global;

public record NugetServer
{
	public Container ReadyToServe { get; init; }


	public NugetServer()
	{
		ReadyToServe = DotnetProject.ToolsContainer()
			.WithMountedDirectory
			(
				"/mnt/bagetter",
				DotnetProject.ToolsContainer()
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

	public async Task<NugetServer> WithPublished(DotnetProject project)
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
