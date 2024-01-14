namespace DaggerSDK;

/// <summary>This error is thrown if the dagger SDK does not identify the error and just wraps the cause.</summary>
public class UnknownDaggerException : DaggerSDKException
{
	public override ErrorCode Code => ErrorCode.UnknownDaggerError;


	internal UnknownDaggerException(string message, DaggerSDKExceptionOptions options)
		: base(message, options)
	{}
}
