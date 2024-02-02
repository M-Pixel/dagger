namespace Dagger;

record EngineSessionConnectionTimeoutExceptionOptions
(
	int TimeOutDurationMs
)
	: DaggerExceptionOptions;

public class EngineSessionConnectionTimeout : DaggerException
{
	public override ErrorCode Code => ErrorCode.EngineSessionConnectionTimeoutError;

	/// <summary>The duration until the timeout occurred in ms.</summary>
	public int TimeOutDurationMs { get; }

	internal EngineSessionConnectionTimeout(string message, EngineSessionConnectionTimeoutExceptionOptions options)
		: base(message, options)
	{
		TimeOutDurationMs = options.TimeOutDurationMs;
	}
}
