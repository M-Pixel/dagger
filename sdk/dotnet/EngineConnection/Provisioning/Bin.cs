using System.Diagnostics;
using System.Text.Json;
using GraphQL.Client.Abstractions;

namespace DaggerSDK;

/// <summary>Runs an engine session from a specified binary</summary>
public class LocalExecutable : IEngineConnection
{
	private Process? _subProcess;

	private string _binPath;


	public LocalExecutable(string binPath)
	{
		_binPath = binPath;
	}

	public void Dispose() => _subProcess?.Dispose();


	public string Address => "http://dagger";

	public Process? SubProcess => _subProcess;


	public async Task<IGraphQLClient> Connect(AdvancedConnectionOptions connectionOptions)
		=> await RunEngineSession(_binPath, connectionOptions);

	public void Close() => _subProcess?.Kill();


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

		_subProcess = Process.Start
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

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		// Log the output if the user wants to.
		if (connectionOptions.LogOutput != null)
			_subProcess.StandardError.BaseStream.CopyToAsync(connectionOptions.LogOutput.BaseStream);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

		const int timeOutDurationMs = 300000;

		connectionOptions.LogOutput?.WriteAsync("OK!\nEstablishing connection to Engine... ");

		EngineConnectionParameters connectionParameters = await await Task.WhenAny<EngineConnectionParameters?>
		(
			ReadConnectionParameters(_subProcess.StandardOutput)!,
			Task.Delay(timeOutDurationMs).ContinueWith(_ => (EngineConnectionParameters?)null)
		)
			?? throw new EngineSessionConnectionTimeout
			(
				"Engine connection timeout",
				new EngineSessionConnectionTimeoutExceptionOptions(TimeOutDurationMs: timeOutDurationMs)
			);

		connectionOptions.LogOutput?.WriteLineAsync("OK!");

		return GraphQLClientFactory.Create(connectionParameters);
	}

	private async Task<EngineConnectionParameters> ReadConnectionParameters(StreamReader stdoutReader)
	{
		if (await stdoutReader.ReadLineAsync() is string line)
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
			await _subProcess!.WaitForExitAsync();
		}
		finally
		{
			throw new EngineSessionException(_subProcess?.ExitCode.ToString() ?? "null sub-process");
		}
	}
}
