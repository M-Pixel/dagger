using System.Diagnostics;
using Dagger;
using GraphQL.Client.Abstractions;
using static System.Environment;
using static Dagger.Bootstrap;
using static Dagger.CodeGenerator;

if (GetCommandLineArgs().Contains("debug"))
	while (Debugger.IsAttached == false)
		await Task.Delay(100);

// TODO: Parse command-line arguments with robust library

// TODO: Use own SDK's Connect, chicken-egg style
(IGraphQLClient client, IEngineConnection? connection) = await CreateGraphQLClient();

// TODO: Progrock integration?

Generator.Configuration configuration = new(OutputDirectory: "../DaggerSDK");

await Generate(configuration, client);

connection?.Dispose();
