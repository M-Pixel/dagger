namespace DaggerSDK;

public class InitEngineSessionBinaryException : DaggerSDKException
{
	public override ErrorCode Code => ErrorCode.InitEngineSessionBinaryError;

	internal InitEngineSessionBinaryException(string Message, DaggerSDKExceptionOptions? options = null)
		: base(Message, options)
	{}
}
