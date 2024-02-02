using System.Diagnostics;
using GraphQL.Client.Abstractions;

namespace Dagger;

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

	private Task<IGraphQLClient>? _client;
	private readonly object _clientCriticalSection = new();

	private Process? _subProcess;
	private bool _disposed;
	private readonly object _disposableCriticalSection = new();


	public Context() {}

	public Context(ContextConfiguration? config)
	{
		if (config?.Client != null)
			_client = Task.FromResult(config.Client);
		_subProcess = config?.SubProcess;
	}

	public void Dispose()
	{
		lock (_disposableCriticalSection)
		{
			_disposed = true;
			_subProcess?.Dispose();
			_subProcess = null;
			_client = null;
		}
	}


	internal Task<IGraphQLClient>? Client
	{
		get
		{
			lock (_disposableCriticalSection)
				lock (_clientCriticalSection)
					return _client;
		}
	}

	/// <summary>Returns a GraphQL client connected to the engine.</summary>
	/// <remarks>If no client is set, it will create one.</remarks>
	public Task<IGraphQLClient> Connection(ConnectionOptions? connectionOptions = null)
	{
		lock (_disposableCriticalSection)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			lock (_clientCriticalSection)
				return _client ??= MakeConnection(connectionOptions);
		}
	}

	/// <summary>Close the connection and the engine if this one was started by the C# SDK.</summary>
	public void Close()
	{
		_subProcess?.Kill();

		// Reset client, so it can restart a new connection if necessary
		_client = null;
	}

	private async Task<IGraphQLClient> MakeConnection(ConnectionOptions? connectionOptions)
	{
		Context defaultContext = await InitializeDefault(connectionOptions);
		lock (_disposableCriticalSection)
		{
			if (_disposed)
			{
				defaultContext._subProcess?.Dispose();
				throw new TaskCanceledException("Context disposed");
			}
			_subProcess = defaultContext._subProcess;
		}
		return await defaultContext._client!;
	}
}
