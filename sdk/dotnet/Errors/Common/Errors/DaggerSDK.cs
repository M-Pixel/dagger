namespace DaggerSDK;
using static Utils;

record DaggerSDKExceptionOptions(Exception? Cause = null);

/// <summary>The base error. Every other error inherits this error.</summary>
public abstract class DaggerSDKException : Exception
{
	/// <summary>The dagger specific error code.  Use this to identify dagger errors programmatically.</summary>
	public abstract ErrorCode Code { get; }

	/// <summary>The original error, which caused the DaggerSDKError.</summary>
	public Exception? cause { get; }


	private protected DaggerSDKException(string message, DaggerSDKExceptionOptions? options = null)
		: base(message)
	{
		cause = options?.Cause;
	}

	/// <summary>Pretty prints the error.</summary>
	void printStackTrace()
	{
		Log(StackTrace);
	}
}
