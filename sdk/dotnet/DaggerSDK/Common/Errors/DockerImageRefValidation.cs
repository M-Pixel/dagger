namespace Dagger;

record DockerImageRefValidationExceptionOptions
(
	string Reference
)
	: DaggerExceptionOptions;

/// <summary>
/// This error is thrown if the passed image reference does not pass validation and is not compliant with the
/// DockerImage constructor.
/// </summary>
public class DockerImageReferenceValidationException : DaggerException
{
	public override ErrorCode Code => ErrorCode.DockerImageRefValidationError;

	/// <summary>
	/// The docker image reference, which caused the error.
	/// </summary>
	public string Reference { get; }

	internal DockerImageReferenceValidationException(string message, DockerImageRefValidationExceptionOptions options)
		: base(message, options)
	{
		Reference = options.Reference;
	}
}
