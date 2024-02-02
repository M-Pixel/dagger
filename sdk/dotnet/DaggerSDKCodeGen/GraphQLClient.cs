using System.Reflection;
using GraphQL.Client.Abstractions;
using static System.Environment;

namespace Dagger;

static class Bootstrap
{
	public static async Task<(IGraphQLClient, IEngineConnection?)> CreateGraphQLClient()
	{
		IGraphQLClient graphQLClient;
		IEngineConnection? engineConnection = null;
		if (GraphQLClientFactory.TryGetParentSession(out EngineConnectionParameters? clientConfiguration))
			graphQLClient = GraphQLClientFactory.Create(clientConfiguration);
		else
		{
			string? executablePath = GetEnvironmentVariable("_EXPERIMENTAL_DAGGER_CLI_BIN");
			if (executablePath == null)
			{
				for
				(
					string? directory = Assembly.GetExecutingAssembly().Location;
					directory != null;
					directory = Path.GetDirectoryName(directory)
				)
				{
					string maybeExecutablePath = Path.Combine(directory, "bin", "dagger");
					if (File.Exists(maybeExecutablePath))
					{
						executablePath = maybeExecutablePath;
						break;
					}
				}

				if (executablePath == null)
					throw new Exception("Could not find Dagger executable.  Did you build it?");
			}

			engineConnection = new LocalExecutable(executablePath);
			graphQLClient =
				await engineConnection.Connect(new AdvancedConnectionOptions(LogOutput: Console.OpenStandardOutput()));
		}

		return (graphQLClient, engineConnection);
	}
}
