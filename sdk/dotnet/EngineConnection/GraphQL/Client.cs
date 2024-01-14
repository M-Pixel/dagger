using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using static System.Environment;

namespace DaggerSDK;

public static class GraphQLClientFactory
{
	public static bool TryGetParentSession([NotNullWhen(true)] out EngineConnectionParameters? parameters)
	{
		string? daggerSessionPort = GetEnvironmentVariable("DAGGER_SESSION_PORT");
		if (string.IsNullOrWhiteSpace(daggerSessionPort))
		{
			parameters = null;
			return false;
		}

		string? sessionToken = GetEnvironmentVariable("DAGGER_SESSION_TOKEN");
		if (string.IsNullOrWhiteSpace(sessionToken))
			throw new Exception("DAGGER_SESSION_TOKEN must be set when using DAGGER_SESSION_PORT");

		parameters = new EngineConnectionParameters(ushort.Parse(daggerSessionPort), sessionToken);
		return true;
	}

	public static IGraphQLWebSocketClient Create(EngineConnectionParameters configuration)
	{
		GraphQLHttpClient result = new
		(
			$"http://127.0.0.1:{configuration.Port}/query",
			new SystemTextJsonSerializer()
		);
		result.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue
		(
			"Basic",
			Convert.ToBase64String(Encoding.UTF8.GetBytes($"{configuration.SessionToken}:"))
		);
		return result;
	}
}
