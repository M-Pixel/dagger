namespace Dagger;
using static Utils;

record DaggerExceptionOptions(Exception? Cause = null);

/// <summary>The base error. Every other error inherits this error.</summary>
public abstract class DaggerException : Exception
{
	/// <summary>The dagger specific error code.  Use this to identify dagger errors programmatically.</summary>
	public abstract ErrorCode Code { get; }

	/// <summary>The original error, which caused the DaggerError.</summary>
	public Exception? cause { get; }


	private protected DaggerException(string message, DaggerExceptionOptions? options = null)
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
