using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dagger;
using static Dagger.Alias;

[
	DirectoryFromContext
	(
		DefaultPath = "/sdk/dotnet",
		Ignore =
		[
			"*",
			"!Client/Client.csproj", "!dagger-icon.png", "!**/*.cs",
			"!Primer/Primer.csproj", "!Primer/**/*.cs",
			"!CodeGenerator/CodeGenerator.csproj", "!CodeGenerator/**/*.cs",
			"!Thunk/Thunk.csproj", "!Thunk/**/*.cs"
		]
	)
]
public static partial class DevelopmentTimeTasks
{
	public static Task TestPublish
	(
		[DirectoryFromContext] Directory source,
		string tag
	)
	{
		string version = tag.Substring("sdk/dotnet/v".Length);
		Secret noSecret = DAG.Secret("no secret");

		return Task.WhenAll
		(
			PublishClient(source.SubDirectory("Client"), noSecret, dryRun: true, version),
			PublishCodeGenerator(source, "no URL", "no user", noSecret, dryRun: true, version),
			PublishPrimer(source, "no URL", "no user", noSecret, dryRun: true, version),
			PublishThunk(source.SubDirectory("Thunk"), "no URL", "no user", noSecret, dryRun: true, version)
		);
	}

	public static async Task PublishFromCD
	(
		[DirectoryFromContext] Directory source,
		string version,
		Secret nugetApiKey,
		string containerRegistryUrl,
		string containerRegistryUsername,
		Secret containerRegistryPassword
	)
	{
		version = version.Substring(1);
		await PublishClient(source.SubDirectory("Client"), nugetApiKey, dryRun: false, version);
		await Task.WhenAll
		(
			PublishCodeGenerator
			(
				source,
				containerRegistryUrl, containerRegistryUsername, containerRegistryPassword,
				dryRun: false,
				version
			),
			PublishPrimer
			(
				source,
				containerRegistryUrl, containerRegistryUsername, containerRegistryPassword,
				dryRun: false,
				version
			),
			PublishThunk
			(
				source,
				containerRegistryUrl, containerRegistryUsername, containerRegistryPassword,
				dryRun: false,
				version
			)
		);
	}

	public static async Task<string> PublishClient
	(
		[
			DirectoryFromContext
			(
				DefaultPath = "/sdk/dotnet/Client",
				Ignore = ["*", "!Client.csproj", "!dagger-icon.png", "!**/*.cs"]
			)
		]
		Directory source,
		Secret key,
		bool dryRun,
		string? version = null
	)
	{
		source = source.WithDirectory("/", await UpdatePackageVersion(source, version ?? await DaggerVerion));

		var packages = DAG.GetBootstrap().ClientPackages(source);
		if (dryRun)
			return "success";
		return await DAG.GetDotnetSdk()
			.DotnetSdkContainer()
			.WithDirectory(".", packages)
			.WithExec([
				"sh", "-c",
				"dotnet nuget push *.nupkg --source=https://api.nuget.org/v3/index.json --no-symbols --skip-duplicate --api-key=" +
				await key.Plaintext()
			])
			.Stdout();
	}

	public static async Task<string> PublishPrimer
	(
		[DirectoryFromContext(Ignore = ["*", "!Primer/Primer.csproj", "!Primer/**/*.cs"])]
		Directory source,
		string url,
		string user,
		Secret secret,
		bool dryRun,
		string? version = null
	)
		=> await (await ContainerWithStaticAnnotations("dagger-dotnet-primer"))
			.WithAnnotation("org.opencontainers.image.title", "Dagger Dotnet SDK Primer")
			.WithAnnotation("org.opencontainers.image.description", "Primes a Dagger Dotnet SDK context for thunking.")
			.WithDirectory("/", DAG.GetBootstrap().Primer(source).Directory("/"))
			.WithDynamicAnnotations()
			.WithRegistryAuth(url, user, secret)
			.Publish($"{url}/dagger-dotnet-primer:{version ?? await DaggerVerion}", dryRun);

	public static async Task<string> PublishCodeGenerator
	(
		[
			DirectoryFromContext
			(
				DefaultPath = "/sdk/dotnet",
				Ignore = ["*", "!CodeGenerator/CodeGenerator.csproj", "!CodeGenerator/**/*.cs"]
			)
		]
		Directory source,
		string url,
		string user,
		Secret secret,
		bool dryRun,
		string? version = null
	)
		=> await (await ContainerWithStaticAnnotations("dagger-dotnet-codegenerator"))
			.WithAnnotation("org.opencontainers.image.title", "Dagger Dotnet SDK Code Generator")
			.WithAnnotation
			(
				"org.opencontainers.image.description",
				"Generates a client library for a Dotnet Dagger module."
			)
			.WithDirectory("/", DAG.GetBootstrap().CodeGenerator(source).Directory("/"))
			.WithDynamicAnnotations()
			.WithRegistryAuth(url, user, secret)
			.Publish($"{url}/dagger-dotnet-codegenerator:{version ?? await DaggerVerion}", dryRun);

	public static async Task<string> PublishThunk
	(
		[DirectoryFromContext(Ignore = ["*", "!Thunk/Thunk.csproj", "!Thunk/**/*.cs"])]
		Directory source,
		string url,
		string user,
		Secret secret,
		bool dryRun,
		string? version = null
	)
		=> await (await ContainerWithStaticAnnotations("dagger-dotnet-thunk"))
			.WithAnnotation("org.opencontainers.image.title", "Dagger Dotnet SDK Thunk")
			.WithAnnotation("org.opencontainers.image.description", "Introspects and invokes Dotnet Dagger modules.")
			.WithDirectory("/", DAG.GetBootstrap().Thunk(source).Directory("/"))
			.WithDynamicAnnotations()
			.WithRegistryAuth(url, user, secret)
			.Publish($"{url}/dagger-dotnet-thunk:{version ?? await DaggerVerion}", dryRun);


	private static async Task<Container> ContainerWithStaticAnnotations(string imageName) => DAG.Container()
		.WithAnnotation("org.opencontainers.image.licenses", "Apache-2.0")
		.WithAnnotation("org.opencontainers.image.documentation", "https://docs.dagger.io/api/custom-functions")
		.WithAnnotation("org.opencontainers.image.authors", "M-Pixel")
		.WithAnnotation
		(
			"org.opencontainers.image.url",
			$"https://github.com/users/M-Pixel/packages/container/package/{imageName}"
		)
		.WithAnnotation("org.opencontainers.image.source", "https://github.com/M-Pixel/dagger")
		.WithAnnotation("org.opencontainers.image.version", await DaggerVerion);
}

static class Extensions
{
	public static Container WithDynamicAnnotations(this Container container) => container
		.WithAnnotation("org.opencontainers.image.created", DateTime.Now.ToString("O"));

	public static Task<string> Publish(this Container container, string address, bool dryRun) => dryRun
		? container.Sync().ContinueWith(_ => "success")
		: container.Publish(address);
}
