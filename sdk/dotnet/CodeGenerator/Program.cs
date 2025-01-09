using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dagger;
using Dagger.Generator;
using Dagger.Introspection;

string? moduleName = Environment.GetEnvironmentVariable("Dagger:Module:Name");
bool isHost = moduleName == null;
if (isHost)
	moduleName = "Host";
string assemblyName = $"Dagger.Generated.{moduleName}";

Console.WriteLine("Parsing schema...");
SchemaDocument document;
FileStream fileStream = new
(
	isHost ? "introspection.json" : "/mnt/introspection.json",
	FileMode.Open, FileAccess.Read, FileShare.ReadWrite
);
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
					Fields = type.Fields.IsDefault ? ImmutableArray<Field>.Empty :
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

Console.WriteLine("Generating client library...");
var syntax = API.Generate(schema, assemblyName);

Console.WriteLine("Compiling...");
ClientCompiler.Compile(syntax);

await disposeTask;
