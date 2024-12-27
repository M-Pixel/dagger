namespace Dagger;

record ExecErrorExceptionOptions
(
	string[] Command,
	int ExitCode,
	string Stdout,
	string Stderr
)
	: DaggerExceptionOptions;

/// <summary>API error from an exec operation in a pipeline.</summary>
public class ExecErrorException : DaggerException
{
	public override ErrorCode Code => ErrorCode.ExecError;

	/// <summary>The command that caused the error.</summary>
	public string[] Command { get; }

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

	public override string ToString() => $"{base.ToString()}\nStdout:\n{Stdout}\nStderr:\n{Stderr}";
}
