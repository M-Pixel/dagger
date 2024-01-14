namespace DaggerSDK;

record EngineSessionConnectParamsParseExceptionOptions
(
	string ParsedLine,
	Exception? Cause = null
)
	: DaggerSDKExceptionOptions(Cause);

public class EngineSessionConnectParamsParseException : DaggerSDKException
{
	public override ErrorCode Code => ErrorCode.EngineSessionConnectParamsParseError;

	/// <summary>The line, which caused the error during parsing, if the error was caused because of parsing.</summary>
	public string ParsedLine { get; }


	internal EngineSessionConnectParamsParseException
	(
		string message,
		EngineSessionConnectParamsParseExceptionOptions options
	)
		: base(message, options)
	{
		ParsedLine = options.ParsedLine;
	}
}
