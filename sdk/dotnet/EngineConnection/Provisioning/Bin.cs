using System.Diagnostics;
using System.Text.Json;
using GraphQL.Client.Abstractions;

namespace DaggerSDK;

/// <summary>Runs an engine session from a specified binary</summary>
public class LocalExecutable : IEngineConnection
{
	private Process? _subProcess;
	private Task<IGraphQLClient>? _client;
	private bool _disposed;
	private readonly object _criticalSection = new();

	private readonly string _binPath;


	public LocalExecutable(string binPath)
	{
		_binPath = binPath;
	}

	public void Dispose()
	{
		lock (_criticalSection)
		{
			_disposed = true;
			_subProcess?.Dispose();
			_subProcess = null;
			_client = null;
		}
	}


	public string Address => "http://dagger";

	public Process? SubProcess
	{
		get
		{
			lock (_criticalSection)
			{
				ObjectDisposedException.ThrowIf(_disposed, this);
				return _subProcess;
			}
		}
	}


	// TODO: This is poorly encapsulated - semantics of parameterized Connect & parameterless Close disagree - should be factory, not method
	public Task<IGraphQLClient> Connect(AdvancedConnectionOptions connectionOptions)
		=> _client ??= RunEngineSession(_binPath, connectionOptions);

	public void Close()
	{
		lock (_criticalSection)
		{
			_subProcess?.Kill();
			_subProcess = null;
			_client = null;
		}
	}


	/// <summary>Execute the engine binary and set up a GraphQL client that target this engine.</summary>
	private async Task<IGraphQLClient> RunEngineSession
	(
		string binPath,
		AdvancedConnectionOptions connectionOptions
	)
	{
		string? sdkVersion = FileVersionInfo.GetVersionInfo(typeof(LocalExecutable).Assembly.Location).FileVersion;

		List<string> arguments = new()
		{
			"session",
			"--label=dagger.io/sdk.name:dotnet",
			$"--label=dagger.io/sdk.version:{sdkVersion}"
		};

		Dictionary<string, string?> flagsAndValues = new()
		{
			{ "workdir", connectionOptions.WorkingDirectory },
			{ "project", connectionOptions.Project }
		};

		connectionOptions.LogOutput?.WriteAsync("Creating new Engine session... ");

		var subProcess = Process.Start
		(
			new ProcessStartInfo
			(
				binPath,
				arguments.Concat(flagsAndValues.Where(x => x.Value != null).Select(x => $"--{x.Key}={x.Value}"))
			)
			{
				RedirectStandardError = true,
				RedirectStandardOutput = true
			}
		)!;

		lock (_criticalSection)
			_subProcess = subProcess;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		// Log the output if the user wants to.
		if (connectionOptions.LogOutput != null)
			_subProcess.StandardError.BaseStream.CopyToAsync(connectionOptions.LogOutput.BaseStream);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

		const int timeOutDurationMs = 300000;

		connectionOptions.LogOutput?.WriteAsync("OK!\nEstablishing connection to Engine... ");

		EngineConnectionParameters connectionParameters =
		(
			await Task.WhenAny<EngineConnectionParameters?>
			(
				ReadConnectionParameters(subProcess)!,
				Task.Delay(timeOutDurationMs).ContinueWith(_ => (EngineConnectionParameters?)null)
			)
		)
			.Result ?? throw new EngineSessionConnectionTimeout
				(
					"Engine connection timeout",
					new EngineSessionConnectionTimeoutExceptionOptions(TimeOutDurationMs: timeOutDurationMs)
				);
		lock (_criticalSection)
			if (_disposed)
				throw new TaskCanceledException("Executable Disposed");
			else if (_subProcess == null)
				throw new TaskCanceledException("Executable closed");

		connectionOptions.LogOutput?.WriteLineAsync("OK!");

		return GraphQLClientFactory.Create(connectionParameters);
	}

	private async Task<EngineConnectionParameters> ReadConnectionParameters(Process subProcess)
	{
		if (await subProcess.StandardOutput.ReadLineAsync() is string line)
		{
			// parse the the line as json-encoded connect params
			try
			{
				return JsonSerializer.Deserialize<EngineConnectionParameters>(line)
				    ?? throw new EngineSessionConnectParamsParseException
					(
						$"invalid connect params: ${line}",
						new EngineSessionConnectParamsParseExceptionOptions(line)
					);
			}
			catch (Exception exception)
			{
				throw new EngineSessionConnectParamsParseException
				(
					$"invalid connect params: ${line}",
					new EngineSessionConnectParamsParseExceptionOptions(line, exception)
				);
			}
		}

		// Need to find a better way to handle this part.
		// At this stage something wrong happened, `ReadLineAsync` didn't return anything await the subprocess to catch
		// the error.
		try
		{
			await subProcess.WaitForExitAsync();
		}
		finally
		{
			throw new EngineSessionException(subProcess.ExitCode.ToString());
		}
	}
}
