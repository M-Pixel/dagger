using GraphQL.Client.Abstractions;

namespace DaggerSDK;
using static Environment;

static class TestHelpers
{
	public static readonly string? SessionToken = GetEnvironmentVariable("DAGGER_SESSION_TOKEN");
	public static readonly string? SessionPort = GetEnvironmentVariable("DAGGER_SESSION_PORT");
	private static LocalExecutable? _executable;
	private static readonly object _mutex = new();

	public static async Task TestParallelConnect(Func<Client, Task> callback, ConnectionOptions? options = null)
	{
		await callback(new Client{ Context = await MakeContext(options) });
	}

	private static async Task<Context> MakeContext(ConnectionOptions? options)
	{
		if (SessionToken != null && SessionPort != null)
			return new
			(
				new ContextConfiguration
				(
					Client: GraphQLClientFactory
						.Create(new EngineConnectionParameters(ushort.Parse(SessionPort), SessionToken))
				)
			);

		lock (_mutex)
		{
			if (_executable == null)
			{
				string? cliBin = GetEnvironmentVariable("_EXPERIMENTAL_DAGGER_CLI_BIN");
				if (cliBin == null)
					throw new Exception("Cannot run tests without _EXPERIMENTAL_DAGGER_CLI_BIN or DAGGER_SESSION");
				_executable = new(cliBin);
			}
		}

		IGraphQLClient client = await _executable.Connect(new AdvancedConnectionOptions(options));
		return new Context(new ContextConfiguration(client));
	}
}
