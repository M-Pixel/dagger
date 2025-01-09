using GraphQL;

namespace Dagger;

/// <summary>
///		Standard Dagger exception types mapped to integers.
/// </summary>
public enum ErrorCode
{
	/// <inheritdoc cref="GraphQLRequestErrorException"/>
	GraphQLRequestError = 100,

	/// <inheritdoc cref="UnknownDaggerException"/>
	UnknownDaggerError = 101,

	/// <inheritdoc cref="TooManyNestedObjectsException"/>
	TooManyNestedObjectsError = 102,

	/// <inheritdoc cref="ExecErrorException"/>
	ExecError = 109
}

record DaggerExceptionOptions(Exception? Cause = null);

/// <summary>The base error. Every other error inherits this error.</summary>
public abstract class DaggerException : Exception
{
	/// <summary>The dagger specific error code.  Use this to identify dagger errors programmatically.</summary>
	public abstract ErrorCode Code { get; }

	/// <summary>The original error, which caused the DaggerError.</summary>
	public Exception? Cause { get; }


	private protected DaggerException(string message, DaggerExceptionOptions? options = null)
		: base(message)
	{
		Cause = options?.Cause;
	}
}

record GraphQLRequestErrorExceptionOptions
(
	IGraphQLResponse Response,
	string Request
) : DaggerExceptionOptions;

/// <summary>
/// This error originates from the dagger engine. It means that some error was thrown and sent back via GraphQL.
/// </summary>
public class GraphQLRequestErrorException : DaggerException
{
	/// <inheritdoc />
	public override ErrorCode Code => ErrorCode.GraphQLRequestError;

	/// <summary>The query and variables which caused the error.</summary>
	public string RequestContext { get; }

	/// <summary>The GraphQL response containing the error.</summary>
	public IGraphQLResponse Response { get; }


	internal GraphQLRequestErrorException(string message, GraphQLRequestErrorExceptionOptions options)
		: base(message, options)
	{
		RequestContext = options.Request;
		Response = options.Response;
	}
}

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
	/// <inheritdoc />
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

/// <summary>This error is thrown if the dagger SDK does not identify the error and just wraps the cause.</summary>
public class UnknownDaggerException : DaggerException
{
	/// <inheritdoc />
	public override ErrorCode Code => ErrorCode.UnknownDaggerError;


	internal UnknownDaggerException(string message, DaggerExceptionOptions options)
		: base(message, options)
	{}
}

record ExecErrorExceptionOptions
(
	IReadOnlyList<object> Command,
	int ExitCode,
	string Stdout,
	string Stderr
)
	: DaggerExceptionOptions;

/// <summary>API error from an exec operation in a pipeline.</summary>
public class ExecErrorException : DaggerException
{
	/// <inheritdoc />
	public override ErrorCode Code => ErrorCode.ExecError;

	/// <summary>The command that caused the error.</summary>
	public IReadOnlyList<object> Command { get; }

	/// <summary>The exit code of the command.</summary>
	public int ExitCode { get; }

	/// <summary>The stdout of the command.</summary>
	public string Stdout { get; }

	/// <summary>The stderr of the command.</summary>
	public string Stderr { get; }


	internal ExecErrorException(string message, ExecErrorExceptionOptions options)
		: base(message, options)
	{
		Command = options.Command;
		ExitCode = options.ExitCode;
		Stdout = options.Stdout;
		Stderr = options.Stderr;
	}

	/// <inheritdoc />
	public override string ToString() => $"{base.ToString()}\nStdout:\n{Stdout}\nStderr:\n{Stderr}";
}
