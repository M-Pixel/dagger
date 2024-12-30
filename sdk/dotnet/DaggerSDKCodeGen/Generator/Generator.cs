using System.Collections.Immutable;
using Dagger.Introspection;
using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dagger;

abstract class Generator
{
	/// <param name="OutputDirectory">Destination directory for generated code.</param>
	public record Configuration
	(
		string OutputDirectory
	);

	public abstract GeneratedState Generate(Schema schema);

	/// <param name="Overlay">Contains generated code to write over the output directory.</param>
	public record GeneratedState
	(
		CompilationUnitSyntax Overlay
	);

	public static void SetSchemaParents(ref Schema schema)
		=> schema = schema with
		{
			Types = schema.Types
				.Select
				(
					type => type with
					{
						Fields = type.Fields.Select(field => field with { ParentObject = type }).ToImmutableArray()
					}
				)
				.ToImmutableArray()
		};

	public static async Task<Schema> Introspect(IGraphQLClient client)
	{
		await using Stream stdin = Console.OpenStandardInput();
		using StreamReader queryReader = new(stdin);
		GraphQLResponse<Response> introspectionResponse =
			await client.SendQueryAsync<Response>(await queryReader.ReadToEndAsync());
		return introspectionResponse.Data.Schema;
	}

	public static async Task Overlay(CompilationUnitSyntax overlay, string outputDirectory)
	{
		await using StreamWriter writer = System.IO.File.CreateText(Path.Combine(outputDirectory, "Generated.cs"));
		overlay.NormalizeWhitespace("\t", "\n").WriteTo(writer);
	}
}
