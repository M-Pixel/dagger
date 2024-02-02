namespace Dagger;

public class InitEngineSessionBinaryException : DaggerException
{
	public override ErrorCode Code => ErrorCode.InitEngineSessionBinaryError;

	internal InitEngineSessionBinaryException(string Message, DaggerExceptionOptions? options = null)
		: base(Message, options)
	{}
}
