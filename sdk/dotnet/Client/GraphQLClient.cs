using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using static System.Environment;

namespace Dagger;

/// <summary>
///		Interface that allows changing how a <see cref="GraphQLClientFactory"/> formulates the <see cref="Uri"/> that it
///		will send Dagger queries to.
/// </summary>
public interface IGraphQLUriFactory
{
	/// <summary>
	///		Produces the <see cref="Uri"/> that the <see cref="IGraphQLWebSocketClient"/> produced by
	///		<see cref="GraphQLClientFactory"/> will send Dagger queries to.
	/// </summary>
	Uri Create();
}

/// <summary>
///		Default implementation of <see cref="IGraphQLUriFactory"/> that follows Dagger conventions.  Subclass to
///		override certain parts of the <see cref="Uri"/> while maintaining default behavior for the other parts.
/// </summary>
public class GraphQLUriFactory : IGraphQLUriFactory
{
	/// <summary>
	///		Environment variable that must be present for Dagger client to know which port to send GraphQL queries to.
	///		Set automatically by the shim that runs the module runtime.  Shouldn't be of concern unless you are trying
	///		something very low-level.
	/// </summary>
	public const string SESSION_PORT_ENVIRONMENT_VARIABLE_KEY = "DAGGER_SESSION_PORT";


	/// <inheritdoc />
	public virtual Uri Create() => new UriBuilder
	{
		Scheme = Scheme,
		Host = Host,
		Port = Port,
		Path = Path
	}
		.Uri;

	/// <inheritdoc cref="System.Uri.Scheme"/>
	public virtual string Scheme => "http";

	/// <inheritdoc cref="System.Uri.Host"/>
	public virtual string Host => "127.0.0.1";

	/// <inheritdoc cref="System.Uri.Port"/>
	/// <exception cref="MissingSessionPortException"></exception>
	/// <exception cref="InvalidSessionPortException"></exception>
	public virtual ushort Port => ushort.TryParse
	(
		GetEnvironmentVariable(SESSION_PORT_ENVIRONMENT_VARIABLE_KEY) ?? throw new MissingSessionPortException(),
		out ushort parsedSessionPort
	)
		? parsedSessionPort
		: throw new InvalidSessionPortException(GetEnvironmentVariable(SESSION_PORT_ENVIRONMENT_VARIABLE_KEY)!);

	/// <inheritdoc cref="System.Uri.AbsolutePath"/>
	public virtual string Path => "query";
}

/// <summary>
///		Interface that allows changing how a <see cref="GraphQLClientFactory"/>'s
///		<see cref="IGraphQLWebsocketJsonSerializer"/> that is used to (de)serialize Dagger queries and responses will be
///		created.
/// </summary>
public interface IGraphQLJsonSerializerFactory
{
	/// <summary>
	///		Produces the <see cref="IGraphQLWebsocketJsonSerializer"/> that will be used to (de)serialize Dagger queries
	///		and responses.
	/// </summary>
	IGraphQLWebsocketJsonSerializer Create();
}

/// <summary>
///		Default implementation of <see cref="IGraphQLJsonSerializerFactory"/> that uses Dotnet's built-in
///		<see cref="JsonSerializer"/> and configures it to properly convert immutable arrays and enums.  Subclass to
///		intercept or override certain aspects the <see cref="IGraphQLWebsocketJsonSerializer"/> while maintaining
///		default behavior for the other aspects.
/// </summary>
public class GraphQLJsonSerializerFactory : IGraphQLJsonSerializerFactory
{
	/// <inheritdoc />
	public IGraphQLWebsocketJsonSerializer Create() => new SystemTextJsonSerializer(Options);

	/// <inheritdoc cref="JsonSerializerDefaults"/>
	public virtual JsonSerializerOptions Options => new()
	{
		Converters = { new ImmutableArrayConverterFactory(), new JsonStringEnumConverter() }
	};
}

/// <summary>
///		Interface that allows changing how a <see cref="Session"/>'s <see cref="IGraphQLWebSocketClient"/> that is used
///		to send Dagger queries will be created.
/// </summary>
public interface IGraphQLClientFactory
{
	/// <summary>
	///		Produces the <see cref="IGraphQLWebSocketClient"/> that a <see cref="Session"/> uses to send Dagger queries.
	/// </summary>
	public IGraphQLWebSocketClient Create();
}

/// <summary>
///		Default implementation of <see cref="IGraphQLClientFactory"/> that follows Dagger conventions.  Subclass to
///		override certain aspects of the <see cref="IGraphQLWebSocketClient"/> while maintaining default behavior for the
///		other aspects.
/// </summary>
public class GraphQLClientFactory : IGraphQLClientFactory
{
	/// <summary>
	///		Environment variable that must be present for Dagger client authorize its connection to the engine.
	///		Set automatically by the shim that runs the module runtime.  Shouldn't be of concern unless you are trying
	///		something very low-level.
	/// </summary>
	public const string SESSION_TOKEN_ENVIRONMENT_VARIABLE_KEY = "DAGGER_SESSION_TOKEN";


	/// <inheritdoc />
	public virtual IGraphQLWebSocketClient Create() => new GraphQLHttpClient
	(
		UriFactory.Create(),
		JsonSerializerFactory.Create()
	)
	{
		HttpClient =
		{
			DefaultRequestHeaders =
			{
				Authorization = new AuthenticationHeaderValue
				(
					"Basic",
					Convert.ToBase64String(Encoding.UTF8.GetBytes($"{SessionToken}:"))
				)
			},
			Timeout = Timeout.InfiniteTimeSpan
		}
	};

	/// <summary>Produces the <see cref="IGraphQLUriFactory"/> that will be used by <see cref="Create"/>.</summary>
	public virtual IGraphQLUriFactory UriFactory => new GraphQLUriFactory();

	/// <summary>The authorization token that is used to authenticate Dagger queries.</summary>
	public virtual string SessionToken => GetEnvironmentVariable(SESSION_TOKEN_ENVIRONMENT_VARIABLE_KEY)
		?? throw new MissingSessionTokenException();

	/// <summary>
	///		The <see cref="IGraphQLWebsocketJsonSerializer"/> that will be used to (de)serialize Dagger queries and
	///		responses.
	/// </summary>
	public virtual IGraphQLJsonSerializerFactory JsonSerializerFactory => new GraphQLJsonSerializerFactory();
}

/// <summary>
///		Thrown when an environment variable that is required for connecting to Dagger's GraphQL API is missing.
/// </summary>
public abstract class MissingEnvironmentVariableException : Exception
{
	private protected MissingEnvironmentVariableException(string environmentVariable)
		: base($"Environment variable {environmentVariable} is empty.  You probably tried connecting from outside of a module.")
	{}
}

/// <summary>
///		Thrown when the environment variable that is required for connecting to Dagger's GraphQL port is missing.
/// </summary>
public class MissingSessionPortException : MissingEnvironmentVariableException
{
	internal MissingSessionPortException() : base(GraphQLUriFactory.SESSION_PORT_ENVIRONMENT_VARIABLE_KEY) {}
}

/// <summary>
///		Thrown when the environment variable that is required for authorizing Dagger queries is missing.
/// </summary>
public class MissingSessionTokenException : MissingEnvironmentVariableException
{
	internal MissingSessionTokenException() : base(GraphQLClientFactory.SESSION_TOKEN_ENVIRONMENT_VARIABLE_KEY) {}
}

/// <summary>
///		Thrown when the environment variable that is required for connecting to Dagger's GraphQL port can't be parsed.
/// </summary>
public class InvalidSessionPortException : Exception
{
	internal InvalidSessionPortException(string value)
		: base($"{value} (from {GraphQLUriFactory.SESSION_PORT_ENVIRONMENT_VARIABLE_KEY}) is not a valid port number.")
	{}
}
