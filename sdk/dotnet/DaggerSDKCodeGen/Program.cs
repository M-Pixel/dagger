using System.Diagnostics;
using Dagger;
using GraphQL.Client.Abstractions;
using static System.Environment;
using static Dagger.CodeGenerator;

if (GetCommandLineArgs().Contains("debug"))
	while (Debugger.IsAttached == false)
		await Task.Delay(100);

// TODO: Parse command-line arguments with robust library

using var context = Context.Default;
IGraphQLClient client = await context.Connection();

// TODO: Progrock integration?

Generator.Configuration configuration = new(OutputDirectory: "../DaggerSDK");

await Generate(configuration, client);
