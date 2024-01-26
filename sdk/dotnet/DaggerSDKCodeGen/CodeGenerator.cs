using DaggerSDK.Introspection;
using GraphQL.Client.Abstractions;

namespace DaggerSDK;

static class CodeGenerator
{
	public static async Task Generate(Generator.Configuration configuration, IGraphQLClient client)
	{
		Console.WriteLine("generating C# SDK client");
		Schema introspectionSchema = await Generator.Introspect(client);
		Generator.GeneratedState generated = Generate(introspectionSchema, configuration);
		await Generator.Overlay(generated.Overlay, configuration.OutputDirectory);
	}

	static Generator.GeneratedState Generate(Schema introspectionSchema, Generator.Configuration configuration)
	{
		Generator.SetSchemaParents(ref introspectionSchema);
		Generator generator = new CSharpGenerator(configuration);
		return generator.Generate(introspectionSchema);
	}
}
