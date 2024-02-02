using GraphQL.Client.Abstractions;

namespace Dagger;
using static Environment;

public sealed partial class Context
{
	/// <summary>Initialize a default client context from environment.</summary>
	internal static async Task<Context> InitializeDefault(ConnectionOptions? connectionOptions = null)
	{
		Context context;

		// Prefer DAGGER_SESSION_PORT if set
		if (GraphQLClientFactory.TryGetParentSession(out EngineConnectionParameters? clientConfiguration))
		{
			if (connectionOptions != null && !string.IsNullOrWhiteSpace(connectionOptions.WorkingDirectory))
			{
				throw new Exception
				(
					"cannot configure workdir for existing session (please use --workdir or host.directory with " +
					"absolute paths instead)"
				);
			}

			context = new Context
			(
				new ContextConfiguration(Client: GraphQLClientFactory.Create(clientConfiguration))
			);
		}
		else
		{
			// Otherwise, prefer _EXPERIMENTAL_DAGGER_CLI_BIN, with fallback behavior of downloading the CLI and using
			// that as the bin.
			string? cliBin = GetEnvironmentVariable("_EXPERIMENTAL_DAGGER_CLI_BIN");
			if (cliBin == null)
			{
				connectionOptions?.LogOutput?.WriteAsync("Downloading CLI... ");
				cliBin = await ExecutableDownloader.DownloadCLI();
				connectionOptions?.LogOutput?.WriteLineAsync("OK!");
			}

			LocalExecutable engineConnection = new(cliBin);
			IGraphQLClient client = await engineConnection.Connect(new AdvancedConnectionOptions(connectionOptions));

			context = new Context(new ContextConfiguration(client, engineConnection.SubProcess));
		}

		return context;
	}
}
