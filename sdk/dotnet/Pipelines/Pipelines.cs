using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dagger;
using static Dagger.Alias;
using static Const;

static class Const
{
	public static readonly string UID = Environment.GetEnvironmentVariable("APP_UID") ??
		throw new Exception("No env:APP_UID");
}

public static class Pipelines
{
	[JsonIgnore] private static readonly Task<string> _version = DAG.Version();

	public static async Task Client
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
		=> await DAG.GetDotnetCli().Project(source).Test().Publish(apiKey: key, noSymbols: true);

	public static async Task<string> Primer
	(
		[DirectoryFromContext(DefaultPath = "/sdk/dotnet/Primer", Ignore = ["*", "!Primer.csproj", "!**/*.cs"])]
		Directory source,
		string url,
		string user,
		Secret secret
	)
		=> await (await ContainerWithStaticAnnotations("dagger-dotnet-primer"))
			.WithAnnotation("org.opencontainers.image.title", "Dagger Dotnet SDK Primer")
			.WithAnnotation("org.opencontainers.image.description", "Primes a Dagger Dotnet SDK context for thunking.")
			.WithEmptyUserDirectory("/Dependencies")
			.WithEmptyUserDirectory("/PrimedState")
			.WithDirectory("/Primer", DAG.GetDotnetCli().Project(source).BuildWithDefaults(), exclude: ["*.pdb"])
			.WithDynamicAnnotations()
			.WithRegistryAuth(url, user, secret)
			.Publish($"{url}/dagger-dotnet-primer:{await _version}");

	public static async Task<string> CodeGenerator
	(
		[
			DirectoryFromContext
			(
				DefaultPath = "/sdk/dotnet/CodeGenerator",
				Ignore = ["*", "!CodeGenerator.csproj", "!**/*.cs"]
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
			.WithEmptyUserDirectory("/Reference")
			.WithDirectory
			(
				"/CodeGenerator",
				DAG.GetDotnetCli().Project(source).BuildWithDefaults(),
				// Primer includes its dependencies, so that it can be downloaded and run without needing the large
				// dotnet SDK or NuGet client.  The other programs on the other hand can rely on Primer to download
				// their dependencies, so that Foo.dll isn't redundantly included in multiple layers/images.  For
				// reasons explained in `Primer.cs`, it needs to put those dependencies in the same folder as
				// `Dagger.Program.dll`.  So: this directory needs to be writable by the user, and it can exclude the
				// dependency dlls which `dotnet build` unavoidably copies into the build output.
				owner: UID,
				include: ["Dagger.CodeGenerator.*"],
				exclude: ["Dagger.CodeGenerator.pdb"]
			)
			.WithDynamicAnnotations()
			.WithRegistryAuth(url, user, secret)
			.Publish($"{url}/dagger-dotnet-codegenerator:{await _version}");

	public static async Task<string> Thunk
	(
		[DirectoryFromContext(DefaultPath = "/sdk/dotnet/Thunk", Ignore = ["*", "!Thunk.csproj", "!**/*.cs"])]
		Directory source,
		string url,
		string user,
		Secret secret
	)
		=> await (await ContainerWithStaticAnnotations("dagger-dotnet-thunk"))
			.WithAnnotation("org.opencontainers.image.title", "Dagger Dotnet SDK Thunk")
			.WithAnnotation("org.opencontainers.image.description", "Introspects and invokes Dotnet Dagger modules.")
			.WithEmptyUserDirectory("/ThunkDependencies")
			.WithDirectory
			(
				"/Thunk",
				DAG.GetDotnetCli().Project(source).BuildWithDefaults(),
				owner: UID,
				include: ["Dagger.Thunk.*"],
				exclude: ["Dagger.Thunk.pdb"]
			)
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
	public static Container WithEmptyUserDirectory(this Container container, string path)
		=> container.WithDirectory(path, DAG.GetDirectory(), owner: UID);

	public static Directory BuildWithDefaults(this DotnetCliDotnetProject project)
		=> project.Build(configuration: "Release", os: "linux");

	public static Container WithDynamicAnnotations(this Container container) => container
		.WithAnnotation("org.opencontainers.image.created", DateTime.Now.ToString("O"));
}
