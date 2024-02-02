namespace Dagger;

record TooManyNestedObjectsExceptionOptions
(
	object Response
)
	: DaggerExceptionOptions;

/// <summary>
/// Dagger only expects one response value from the engine. If the engine returns more than one value this error is
/// thrown.
/// </summary>
public class TooManyNestedObjectsException : DaggerException
{
	public override ErrorCode Code => ErrorCode.TooManyNestedObjectsError;

	/// <summary>
	/// The response containing more than one value.
	/// </summary>
	public object Response { get; }


	internal TooManyNestedObjectsException(string message, TooManyNestedObjectsExceptionOptions options)
		: base(message, options)
	{
		Response = options.Response;
	}
}
