using System;

namespace Dagger.DotnetCLI;

static class Global
{
	public static string UID = Environment.GetEnvironmentVariable("APP_UID")
		?? throw new Exception("APP_UID not in env (official Dotnet container images have it)");
}
