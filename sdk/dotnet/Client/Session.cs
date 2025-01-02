using GraphQL.Client.Abstractions;

namespace Dagger;

/// <summary>
///		Owns a connection to the Dagger Engine.  Every connection is associated with a particular "session" that is
///		sanctioned by the engine ahead of time.  No <see cref="ObjectClient"/> can exist without a contextual
///		<see cref="Session"/>.
/// </summary>
/// <remarks>Analogous to "Context" in other SDK implementations.</remarks>
public sealed class Session : IDisposable
{
	private readonly IGraphQLClientFactory _graphQLClientFactory;

	private IGraphQLClient? _graphQLClient;
	private readonly object _clientMutex = new();

	private bool _disposed;
	private readonly object _disposableMutex = new();


	/// <summary>Uses the default <see cref="GraphQLClientFactory"/>.</summary>
	public Session() : this(new GraphQLClientFactory()) {}

	/// <summary>Uses your custom <see cref="IGraphQLClientFactory"/></summary>
	public Session(IGraphQLClientFactory graphQLGraphQLClientFactory)
	{
		_graphQLClientFactory = graphQLGraphQLClientFactory;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		lock (_disposableMutex)
		{
			_disposed = true;
			Close();
		}
	}


	/// <summary>Whether a GraphQL connection currently exists or is pending.</summary>
	public bool IsActive => _graphQLClient != null;


	/// <summary>
	///		Returns a GraphQL client connected to the engine.  If no client is set, it will create one.
	/// </summary>
	/// <remarks>
	///		Useful for low-level intervention, but most of the time you should not directly interact with the GraphQL
	///		client (use Query instead).
	/// </remarks>
	public IGraphQLClient AcquireGraphQLClient()
	{
		lock (_disposableMutex)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			lock (_clientMutex)
				return _graphQLClient ??= _graphQLClientFactory.Create();
		}
	}

	/// <summary>Close the GraphQL websocket until it is lazily recreated upon the next query computation.</summary>
	/// <remarks>
	///		Due to limitation from third-party library, the connection won't actually close until it is
	///		garbage-collected.
	/// </remarks>
	public void Close()
	{
		lock (_clientMutex)
		{
			// TODO: IGraphQLClient should be disposable (requires PR to upstream library)
			_graphQLClient = null;
		}
	}
}
