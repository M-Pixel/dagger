using System.Collections.Immutable;
using DaggerSDK.GraphQL;
using DaggerSDKCodeGen;
using DaggerSDKCodeGen.Models;
using System.Text.Json;

JsonSerializerOptions jsonSerializerOptions = new()
{
	PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	WriteIndented = true
};

GraphQLClient client = new();
HttpResponseMessage introspectionResponse = await client.RequestAsync(IntrospectionQuery.Query);
string introspectionResponseBody = await introspectionResponse.Content.ReadAsStringAsync();
var doc = JsonDocument.Parse(introspectionResponseBody);
JsonElement schema = doc.RootElement.GetProperty("data").GetProperty("__schema");

var directives = schema.GetProperty("directives").Deserialize<ImmutableList<QueryDirective>>(jsonSerializerOptions)
	?? throw new Exception("Failed to deserialize directives");
var types = schema.GetProperty("types").Deserialize<ImmutableList<QueryType>>(jsonSerializerOptions)
	?? throw new Exception("Failed to deserialize types");

Console.WriteLine("Writing introspect-api.json");
File.WriteAllText("introspect-api.json", JsonSerializer.Serialize(new {
	directives = schema.GetProperty("directives"),
	types = schema.GetProperty("types")
}, jsonSerializerOptions));

Console.WriteLine("Writing introspect-resparsedult.json");
File.WriteAllText("introspect-parsed.json", JsonSerializer.Serialize(new
{
	directives,
	types
}, jsonSerializerOptions));

Console.WriteLine("Directives extracted: {0}", directives.Count);
Console.WriteLine("Types extracted: {0}", types.Count);
