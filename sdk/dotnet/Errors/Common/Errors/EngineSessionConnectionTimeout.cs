namespace DaggerSDK;

record EngineSessionConnectionTimeoutExceptionOptions
(
	int TimeOutDurationMs
)
	: DaggerSDKExceptionOptions;

public class EngineSessionConnectionTimeout : DaggerSDKException
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
