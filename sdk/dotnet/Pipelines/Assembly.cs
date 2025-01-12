using Dagger;
using Dagger.Dev;

[
	assembly: DirectoryFromContext
	(
		DefaultPath = "/sdk/dotnet/Client",
		Ignore = ["*", "!**/*.csproj", "!**/*.cs", "!" + DotnetSDKProject.ICON_PATH, "**/opt", "**/bin"]
	)
]
