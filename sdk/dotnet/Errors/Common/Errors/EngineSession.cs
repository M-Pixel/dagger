namespace Dagger;

/// <summary>
/// This error is thrown if the EngineSession does not manage to parse the required port successfully because a EOF is
/// read before any valid port.
/// </summary>
/// <remarks>
/// This usually happens if no connection can be established.
/// </remarks>
public class EngineSessionException : DaggerException
{
	public override ErrorCode Code => ErrorCode.EngineSessionError;

	internal EngineSessionException(string message, DaggerExceptionOptions? options = null)
		: base(message, options)
	{}
}
