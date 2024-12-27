namespace Dagger;

/// <summary>This error is thrown when the compute function isn't awaited.</summary>
public class NotAwaitedRequestException : DaggerException
{
	public override ErrorCode Code => ErrorCode.NotAwaitedRequestError;


	internal NotAwaitedRequestException(string message, DaggerExceptionOptions options)
		: base(message, options)
	{}
}
