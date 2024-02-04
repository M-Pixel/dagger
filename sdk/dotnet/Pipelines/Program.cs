using Dagger;
using static System.Environment;

string iconPath = "DaggerSDK/dagger-icon.png";

var dagger = Client.Default
	.Pipeline("Built, Test, and Publish C# SDK");

string apiKey = GetEnvironmentVariable("nugetApiKey") ?? throw new Exception("Need nugetApiKey in env");
dagger.SetSecret("NuGet API Key", apiKey);

Container containerWithNuspec = dagger
	.Container()
	.From("mcr.microsoft.com/dotnet/sdk:8.0-alpine3.19")
	.WithEnvVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
	.WithWorkdir("/home/app/source")
	.WithDirectory
	(
		".",
		dagger.GetHost()
			.Directory(CurrentDirectory, include: ["**/*.csproj"], exclude: ["**/opt/", "**/bin/", "Pipelines/**"])
	)
	.WithExec(["dotnet", "restore", "DaggerSDK/DaggerSDK.csproj"])
	.WithDirectory
	(
		".",
		dagger.GetHost()
			.Directory(CurrentDirectory, include: ["**/*.cs"], exclude: ["**/opt/", "**/bin/"])
	)
	.WithExec(["dotnet", "test", "Tests/IntegrationTests/IntegrationTests.csproj"], experimentalPrivilegedNesting: true)
	.WithFile(iconPath, dagger.GetHost().File($"{CurrentDirectory}/{iconPath}"))
	.WithExec(["dotnet", "pack", "--include-symbols", "DaggerSDK/DaggerSDK.csproj"]);
string nuspecFileName =
	(
		await containerWithNuspec
			.WithExec(["ash", "-c", "ls DaggerSDK/bin/Release/*.nupkg"])
			.Stdout()
	)
	.Split('\n')
	.Select(fileName => fileName.Trim())
	.First(fileName => !fileName.EndsWith(".symbols.nupkg"));
await containerWithNuspec
	.WithExec
	(
		[
			"dotnet", "nuget", "push", nuspecFileName, $"--api-key={apiKey}",
			"--source=https://api.nuget.org/v3/index.json"
		]
	)
	.Sync();
