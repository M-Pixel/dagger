using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dagger;
using static Dagger.Alias;

public static class Pipelines
{
	[JsonIgnore] private static readonly Task<string> _version = DAG.Version();

	public static async Task<string> Client
	(
		[
			DirectoryFromContext
			(
				DefaultPath = "/sdk/dotnet/Client",
				Ignore = ["*", "!Client.csproj", "!dagger-icon.png", "!**/*.cs"]
			)
		]
		Directory source,
		Secret key
	)
		=> await DAG.GetDotnetSdk()
			.DotnetSdkContainer()
			.WithDirectory(".", DAG.GetBootstrap().ClientPackages(source))
			.WithExec(["sh", "-c", "dotnet nuget push *.nupkg --source=https://api.nuget.org/v3/index.json --no-symbols --api-key=" + await key.Plaintext()])
			.Stdout();

	public static async Task<string> Primer
	(
		[DirectoryFromContext(DefaultPath = "/sdk/dotnet", Ignore = ["*", "!Primer/Primer.csproj", "!Primer/**/*.cs"])]
		Directory source,
		string url,
		string user,
		Secret secret
	)
		=> await (await ContainerWithStaticAnnotations("dagger-dotnet-primer"))
			.WithAnnotation("org.opencontainers.image.title", "Dagger Dotnet SDK Primer")
			.WithAnnotation("org.opencontainers.image.description", "Primes a Dagger Dotnet SDK context for thunking.")
			.WithDirectory("/", DAG.GetBootstrap().Primer(source).Directory("/"))
			.WithDynamicAnnotations()
			.WithRegistryAuth(url, user, secret)
			.Publish($"{url}/dagger-dotnet-primer:{await _version}");

	public static async Task<string> CodeGenerator
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
		Secret secret
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
			.Publish($"{url}/dagger-dotnet-codegenerator:{await _version}");

	public static async Task<string> Thunk
	(
		[DirectoryFromContext(DefaultPath = "/sdk/dotnet", Ignore = ["*", "!Thunk/Thunk.csproj", "!Thunk/**/*.cs"])]
		Directory source,
		string url,
		string user,
		Secret secret
	)
		=> await (await ContainerWithStaticAnnotations("dagger-dotnet-thunk"))
			.WithAnnotation("org.opencontainers.image.title", "Dagger Dotnet SDK Thunk")
			.WithAnnotation("org.opencontainers.image.description", "Introspects and invokes Dotnet Dagger modules.")
			.WithDirectory("/", DAG.GetBootstrap().Thunk(source).Directory("/"))
			.WithDynamicAnnotations()
			.WithRegistryAuth(url, user, secret)
			.Publish($"{url}/dagger-dotnet-thunk:{await _version}");


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
		.WithAnnotation("org.opencontainers.image.version", await _version);
}

static class Extensions
{
	public static Container WithDynamicAnnotations(this Container container) => container
		.WithAnnotation("org.opencontainers.image.created", DateTime.Now.ToString("O"));
}
