using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dagger.DotnetCLI;
using static Alias;
using static Global;

/// <summary>A statically typed Dotnet Project (csproj).</summary>
[DirectoryFromContext(Ignore = ["obj", "bin", ".*", "dagger.json"])]
public record DotnetProject
{
	internal Container Container { get; init; }
	private readonly Dictionary<string, string> _msbuildProperties = new();


	public DotnetProject(Directory sourceCodeDirectory)
	{
		Container = ToolsContainer()
			.WithDirectory(".", sourceCodeDirectory, include: ["*.csproj"], owner: UID)
			.WithExec(["dotnet", "restore"])
			.WithDirectory(".", sourceCodeDirectory, exclude: ["obj", "bin", "*.csproj"], owner: UID);
	}


	/// <summary>A container with the Dotnet SDK (not the Dotnet Dagger SDK, the Microsoft Dotnet SDK).</summary>
	public static Container ToolsContainer() => DAG
		.Container()
		.From("mcr.microsoft.com/dotnet/sdk:8.0-noble") // vs Alpine it's only ~34 MiB extra
		.WithExec(["dotnet", "workload", "update"])
		.WithUser(UID)
		.WithEnvVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
		.WithMountedTemp("/tmp")
		.WithMountedCache
		(
			"/home/app/.local/share/NuGet/http-cache",
			DAG.CacheVolume("nuget-http"),
			owner: UID,
			sharing: CacheSharingMode.SHARED
		)
		.WithWorkdir("/scratch")
		.WithDirectory(".", DAG.GetDirectory(), owner: UID);

	/// <summary>Turn a Dotnet project directory into a DotnetProject Dagger object.  Performs a restore automatically.</summary>
	/// <param name="sourceCodeDirectory">Should contain a <c>.csproj</c>.</param>
	public static DotnetProject Project([DirectoryFromContext] Directory sourceCodeDirectory)
		=> new(sourceCodeDirectory);


	/// <summary>
	///		Set or override these project-level properties.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public DotnetProject WithProperty(string key, string value)
	{
		_msbuildProperties[key] = value;
		return this;
	}

	/// <summary>Build a .NET project.</summary>
	/// <param name="useCurrentRuntime">Use current runtime as the target runtime.</param>
	/// <param name="framework">The target framework to build for. The target framework must also be specified in the project file.</param>
	/// <param name="configuration">The configuration to use for building the project. The default for most projects is 'Debug'.</param>
	/// <param name="runtime">The target runtime to build for.</param>
	/// <param name="versionSuffix">Set the value of the <c>$(VersionSuffix)</c> property to use when building the project.</param>
	/// <param name="verbosity">Set the MSBuild verbosity level.</param>
	/// <param name="selfContained">Publish the .NET runtime with your application so the runtime doesn't need to be installed on the target machine.</param>
	/// <param name="arch">The target architecture.</param>
	/// <param name="os">The target operating system.</param>
	/// <param name="warnAsError">List of warning codes to treats as errors.  To treat all warnings as errors, specify "all".</param>
	/// <param name="warnNotAsError">List of warning codes to not treat as errors.</param>
	/// <param name="warnAsMessage">List of warning codes to treat as low-importance messages.</param>
	/// <param name="detailedSummary">Shows detailed information at the end of the build about the configurations built and how they were scheduled to nodes.</param>
	public Directory Build
	(
		bool useCurrentRuntime = false,
		string? framework = null,
		string? configuration = null,
		string? runtime = null,
		string? versionSuffix = null,
		string? verbosity = null, // TODO: Use enum
		bool selfContained = false,
		string? arch = null,
		string? os = null,
		IReadOnlyCollection<string>? warnAsError = null,
		IEnumerable<string>? warnNotAsError = null,
		IEnumerable<string>? warnAsMessage = null,
		bool detailedSummary = false
	)
	{
		List<string> command = new()
		{
			"dotnet", "build", "--nologo", "--no-restore", "--output=./out", "-maxCpuCount",
			"-p:ContinuousIntegrationBuild=true"
		};
		if (useCurrentRuntime)
			command.Add("--ucr");
		if (framework != null)
			command.Add($"--framework={framework}");
		if (configuration != null)
			command.Add($"--configuration={configuration}");
		if (runtime != null)
			command.Add($"--runtime={runtime}");
		if (versionSuffix != null)
			command.Add($"--version-suffix={versionSuffix}");
		if (verbosity != null)
			command.Add($"--verbosity={verbosity}");
		if (selfContained)
			command.Add("--sc");
		if (arch != null)
			command.Add($"--arch={arch}");
		if (os != null)
			command.Add($"--os={os}");
		if (warnAsError?.Contains("all") ?? false)
			command.Add("-warnAsError");
		else if (warnAsError != null)
			command.Add("-warnAsError:" + string.Join(';', warnAsError));
		if (warnNotAsError != null)
			command.Add("-warnNotAsError:" + string.Join(';', warnNotAsError));
		if (warnAsMessage != null)
			command.Add("-noWarn:" + string.Join(';', warnAsMessage));
		foreach (KeyValuePair<string,string> msbuildProperty in _msbuildProperties)
			command.Add($"-p:{msbuildProperty.Key}={msbuildProperty.Value}");
		if (detailedSummary)
			command.Add("-ds:true");
		return Container
			.WithExec(command)
			.Directory("out");
	}

	public Task<TestedDotnetProject> Test()
	{
		// TODO: Implement testing
		return Task.FromResult(new TestedDotnetProject(this));
	}

	/// <summary>Create a NuGet package.</summary>
	/// <param name="includeSymbols">Include packages with symbols in addition to regular packages in output directory.</param>
	/// <param name="includeSource">Include PDBs and source files.  Source files go into the 'src' folder in the resulting nuget package.</param>
	/// <param name="serviceable">Set the serviceable flag in the package.  See https://aka.ms/nupkgservicing for more information.</param>
	/// <param name="verbosity">Set the MSBuild verbosity level.</param>
	/// <param name="versionSuffix">Set the value of the <c>$(VersionSuffix)</c> property to use when building the project.</param>
	/// <param name="configuration">The configuration to use for building the package.  The default is 'Release'.</param>
	/// <param name="useCurrentRuntime">Use current runtime as the target runtime.</param>
	public Directory Pack
	(
		bool includeSymbols = false,
		bool includeSource = false,
		bool serviceable = false,
		string? versionSuffix = null,
		string? verbosity = null, // TODO: Use enum
		string? configuration = null,
		bool useCurrentRuntime = false
	)
	{
		List<string> command = new(){ "dotnet", "--nologo", "pack", "--no-restore" };
		if (includeSymbols)
			command.Add("--include-symbols");
		if (includeSource)
			command.Add("--include-source");
		if (serviceable)
			command.Add("-s");
		if (verbosity != null)
			command.Add($"--verbosity={verbosity}");
		if (versionSuffix != null)
			command.Add($"--version-suffix={versionSuffix}");
		if (configuration != null)
			command.Add($"--configuration={configuration}");
		if (useCurrentRuntime)
			command.Add("--ucr");
		return DAG.GetDirectory().WithDirectory
		(
			".",
			Container
				.WithExec(command)
				.Directory("bin/Release"),
			include: ["*.nupkg"]
		);
	}

	/// <summary>
	///		Publish the untested project to a NuGet-server-as-Dagger-service.  To publish to the internet, test the
	///		project first.
	/// </summary>
	/// <param name="toService">A service that should be running a NuGet server.</param>
	/// <param name="port">If not specified will use the first published port of the service.</param>
	public async Task Publish(Service toService, int port = -1)
	{
		string portString = (port == -1 ? await (await toService.Ports()).First().SubPort() : port).ToString();
		await
			(
				this with
				{
					Container = Container.WithServiceBinding("nuget", toService)
				}
			)
			.Publish(new Uri("http://nuget:" + portString));
	}

	/// <summary>
	///		Publishes the project to a local NuGet server, and returns that server as a service.  Run this and add
	///		&lt;add key="local" value="http://localhost:8080/v3/index.json" allowInsecureConnections="true" /&gt; to
	///		<c>nuget.config</c> to test installing the latest local iteration of your project into other projects.
	/// </summary>
	public async Task<Service> Serve() => (await new NugetServer().WithPublished(this)).AsService();


	internal async Task Publish
	(
		Uri nugetServer,
		Task<IEnumerable<string>>? additionalArgumentsTask = null
	)
	{
		Directory packages = Pack();
		await Task.WhenAll
		(
			(await packages.Entries()).Select<string, Task>
			(
				packageName => Task.Run
				(
					async () =>
					{
						IEnumerable<string> additional = additionalArgumentsTask != null
							? await additionalArgumentsTask
							: [];
						await ToolsContainer()
							.WithMountedDirectory(".", packages)
							.WithExec
							(
								["dotnet", "nuget", "push", packageName, ..additional, $"--source={nugetServer}"]
							)
							.Sync();
					}
				)
			)
		);
	}
}

public record TestedDotnetProject
{
	public DotnetProject Project { get; init; }


	internal TestedDotnetProject(DotnetProject project)
	{
		Project = project;
	}


	public Directory Pack() => Project.Pack();

	/// <summary>Publish a .NET project for deployment.</summary>
	/// <param name="source">Package source (URL) to use. Defaults to https://api.nuget.org/v3/index.json.</param>
	/// <param name="symbolSource">Symbol server URL to use.</param>
	/// <param name="timeout">Timeout for pushing to a server in seconds.  Defaults to 300 seconds (5 minutes).</param>
	/// <param name="apiKey">The API key for the server.</param>
	/// <param name="symbolApiKey">The API key for the symbol server.</param>
	/// <param name="disableBuffering">Disable buffering when pushing to an HTTP(S) server to decrease memory usage.</param>
	/// <param name="noSymbols">If a symbols package exists, it will not be pushed to a symbols server.</param>
	/// <param name="noServiceEndpoint">Does not append "api/v2/package" to the source URL.</param>
	/// <param name="skipDuplicate">If a package and version already exists, skip it and continue with the next package in the push, if any.</param>
	/// <returns>If a package and version already exists, skip it and continue with the next package in the push, if any.</returns>
	public Task Publish
	(
		string? source = null,
		string? symbolSource = null,
		int timeout = 0,
		Secret? apiKey = null,
		Secret? symbolApiKey = null,
		bool disableBuffering = false,
		bool noSymbols = false,
		bool noServiceEndpoint = false,
		bool skipDuplicate = false
	)
	{
		Task<IEnumerable<string>> additionalArgumentsTask = ((Func<Task<IEnumerable<string>>>)(async () => {
			List<string> additionalArguments = new();
			if (source != null)
				additionalArguments.Add($"--source={source}");
			if (symbolSource != null)
				additionalArguments.Add($"--symbol-source={symbolSource}");
			if (timeout > 0)
				additionalArguments.Add($"--timeout={timeout}");
			if (apiKey != null)
				additionalArguments.Add($"--api-key={await apiKey.Plaintext()}");
			if (symbolApiKey != null)
				additionalArguments.Add($"--symbol-api-key={await symbolApiKey.Plaintext()}");
			if (disableBuffering)
				additionalArguments.Add("-d");
			if (noSymbols)
				additionalArguments.Add("-n");
			if (noServiceEndpoint)
				additionalArguments.Add("--no-service-endpoint");
			if (skipDuplicate)
				additionalArguments.Add("--skip-duplicate");
			return additionalArguments;
		}))();
		return Project.Publish(new Uri("https://api.nuget.org/v3/index.json"), additionalArgumentsTask);
	}
}
