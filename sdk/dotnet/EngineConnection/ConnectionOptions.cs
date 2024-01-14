namespace DaggerSDK;

/// <summary>
/// Defines options used to connect to an engine.
/// </summary>
/// <param name="WorkingDirectory">Use to overwrite Dagger workdir.  Default is that of the SDK process.</param>
/// <param name="LogOutput">Enable logs output.</param>
public record ConnectionOptions
(
	string? WorkingDirectory = null,
	StreamWriter? LogOutput = null
)
	: IDisposable, IAsyncDisposable
{
	public ConnectionOptions(string? WorkingDirectory, Stream? LogOutput)
		: this
		(
			WorkingDirectory,
			LogOutput == null ? null : new StreamWriter(LogOutput, Console.OutputEncoding)
		)
	{}

	public ConnectionOptions(Stream LogOutput)
		: this(null, LogOutput)
	{}


	public void Dispose() => LogOutput?.Dispose();
	public ValueTask DisposeAsync() => LogOutput?.DisposeAsync() ?? ValueTask.CompletedTask;
}
