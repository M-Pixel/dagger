namespace Dagger;

/// <summary>This error is thrown if the dagger SDK does not identify the error and just wraps the cause.</summary>
public class UnknownDaggerException : DaggerException
{
	public override ErrorCode Code => ErrorCode.UnknownDaggerError;


	internal UnknownDaggerException(string message, DaggerExceptionOptions options)
		: base(message, options)
	{}
}
