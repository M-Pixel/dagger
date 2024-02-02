using System.Text.Json.Serialization;
using GraphQL.Client.Abstractions;

namespace Dagger;

public record AdvancedConnectionOptions : ConnectionOptions
{
	public readonly string? Project;
	public readonly uint? TimeoutMs;


	public AdvancedConnectionOptions
	(
		string? WorkingDirectory = null,
		Stream? LogOutput = null,
		string? Project = null,
		uint? TimeoutMs = null
	)
		: base(WorkingDirectory, LogOutput)
	{
		this.Project = Project;
		this.TimeoutMs = TimeoutMs;
	}

	public AdvancedConnectionOptions(ConnectionOptions? connectionOptions = null)
		: base(connectionOptions?.WorkingDirectory, connectionOptions?.LogOutput)
	{}
}

public record EngineConnectionParameters
(
	[property: JsonPropertyName("port")]
	ushort Port,

	[property: JsonPropertyName("session_token")]
	string SessionToken
);

public interface IEngineConnection : IDisposable
{
	/// <summary>The connector address.</summary>
	string Address { get; }

	/// <summary>Initializes a ready to use GraphQL Client that points to the engine.</summary>
	Task<IGraphQLClient> Connect(AdvancedConnectionOptions connectionOptions);

	/// <summary>Stops the current connection.</summary>
	void Close();
}
