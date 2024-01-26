using System.Diagnostics;
using GraphQL.Client.Abstractions;

namespace DaggerSDK;

public record ContextConfiguration
(
	IGraphQLClient? Client = null,
	Process? SubProcess = null
);

/// <summary>Context abstracts the connection to the engine.</summary>
/// <remarks>
/// <para>
/// It's required to implement the default global SDK.  Its purpose is to store and returns the connection to the
/// graphQL API, if no connection is set, it can create its own.
/// </para>
/// <para>
/// This is also useful for lazy evaluation with the default global client, this one should only run the engine if it
/// actually executes something.
/// </para>
/// </remarks>
public sealed partial class Context : IDisposable
{
	public static Context Default { get; } = new();
	private IGraphQLClient? _client;
	private Process? _subProcess;


	public Context() {}

	public Context(ContextConfiguration? config)
	{
		if (config != null)
			(_client, _subProcess) = config;
	}

	public void Dispose()
	{
		_subProcess?.Dispose();
	}


	/// <summary>Returns a GraphQL client connected to the engine.</summary>
	/// <remarks>If no client is set, it will create one.</remarks>
	public async Task<IGraphQLClient> Connection(ConnectionOptions? connectionOptions = null)
	{
		if (_client == null)
		{
			Context defaultContext = await InitializeDefault(connectionOptions);
			_client = defaultContext._client!;
			_subProcess = defaultContext._subProcess;
		}

		return _client;
	}

	/// <summary>Close the connection and the engine if this one was started by the C# SDK.</summary>
	public void Close()
	{
		_subProcess?.Kill();

		// Reset client, so it can restart a new connection if necessary
		_client = null;
	}
}
