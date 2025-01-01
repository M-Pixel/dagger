using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dagger;
using Dagger.Generator;
using Dagger.Introspection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static System.IO.File;

Console.WriteLine("Parsing schema...");
SchemaDocument document;
FileStream fileStream = new("introspection.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
{
	JsonSerializerOptions serializerOptions = new()
	{
		Converters = { new ImmutableArrayConverterFactory(), new JsonStringEnumConverter() },
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};
	document = await JsonSerializer.DeserializeAsync<SchemaDocument>(fileStream, serializerOptions)
		?? throw new Exception
			(
				"Deserializing introspection.json unexpectedly produced null result instead of crashing or succeeding"
			);
}
ValueTask disposeTask = fileStream.DisposeAsync();

Console.WriteLine("Post-processing schema...");
Schema schema = document.Schema with
{
	Types =
	[
		..document.Schema.Types
			.Select
			(
				type => type with
				{
					Fields =
					[
						..type.Fields
							.Select(field => field with { ParentObject = type })
							.OrderBy(queryField => queryField.Name, FieldSorter.instance)
					]
				}
			)
			.OrderBy(queryType => queryType.Name)
	]
};

Console.WriteLine("Generating C# SDK structure...");
CompilationUnitSyntax generatedSyntax = API.Generate(schema);

Console.WriteLine("Writing C# SDK source code...");
StreamWriter writer = CreateText("Generated.cs");
generatedSyntax.NormalizeWhitespace("\t", "\n").WriteTo(writer);
await writer.DisposeAsync();

await disposeTask;
