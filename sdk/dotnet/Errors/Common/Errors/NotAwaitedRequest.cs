namespace DaggerSDK;

/// <summary>This error is thrown when the compute function isn't awaited.</summary>
public class NotAwaitedRequestException : DaggerSDKException
{
	public override ErrorCode Code => ErrorCode.NotAwaitedRequestError;


	internal NotAwaitedRequestException(string message, DaggerSDKExceptionOptions options)
		: base(message, options)
	{}
}
