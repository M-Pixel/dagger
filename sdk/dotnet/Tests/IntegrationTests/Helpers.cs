using GraphQL.Client.Abstractions;
using static System.Environment;

namespace Dagger.IntegrationTests;

static class TestHelpers
{
	public static readonly string? SessionToken = GetEnvironmentVariable("DAGGER_SESSION_TOKEN");
	public static readonly string? SessionPort = GetEnvironmentVariable("DAGGER_SESSION_PORT");

	public static async Task TestParallelConnect(Func<Query, Task> callback)
	{
		await callback(new Query{ Session = new Session() });
	}
}
