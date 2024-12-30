using System.Collections.Immutable;
using System.Diagnostics;
using Dagger;
using Dagger.Generator;
using Dagger.Introspection;
using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static System.Environment;

if (GetCommandLineArgs().Contains("debug"))
	while (Debugger.IsAttached == false)
		await Task.Delay(100);

Console.WriteLine("Connecting to Dagger...");
using var context = Context.Default;
Task<IGraphQLClient> clientTask = context.Connection();

// Read query from stdin in parallel
string introspectionQuery;
{
	await using Stream stdin = Console.OpenStandardInput();
	using StreamReader queryReader = new(stdin);
	introspectionQuery = await queryReader.ReadToEndAsync();
}
IGraphQLClient client = await clientTask;

Console.WriteLine("Obtaining schema...");
GraphQLResponse<Response> introspectionResponse = await client.SendQueryAsync<Response>(introspectionQuery);
Schema schema = introspectionResponse.Data.Schema
	.SetParents();
schema = schema with
{
	Types =
	[
		..schema.Types
			.Select
			(
				type => type with
				{
					Fields = [..type.Fields.OrderBy(queryField => queryField.Name, FieldSorter.instance)]
				}
			)
			.OrderBy(queryType => queryType.Name)
	]
};

Console.WriteLine("Generating C# SDK structure...");
CompilationUnitSyntax generatedSyntax = API.Generate(schema);

await using StreamWriter writer = System.IO.File.CreateText("../DaggerSDK/Generated.cs");
generatedSyntax.NormalizeWhitespace("\t", "\n").WriteTo(writer);

