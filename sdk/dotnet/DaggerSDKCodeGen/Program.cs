using System.Collections.Immutable;
using DaggerSDKCodeGen.Models;
using System.Text.Json;
using DaggerSDK;
using GraphQL;
using GraphQL.Client.Abstractions;
using static System.Environment;
using static DaggerSDK.CodeGen.Statics;

JsonSerializerOptions jsonSerializerOptions = new()
{
	PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	WriteIndented = true
};

(IGraphQLClient client, IEngineConnection? connection) = await CreateGraphQLClient();

GraphQLResponse<JsonDocument> introspectionResponse = await client.SendQueryAsync<JsonDocument>
(
	new GraphQLRequest(File.ReadAllText("../../../cmd/codegen/introspection/introspection.graphql"))
);
JsonDocument doc = introspectionResponse.Data;
JsonElement schema = doc.RootElement.GetProperty("__schema");

var directives = schema.GetProperty("directives").Deserialize<ImmutableList<QueryDirective>>(jsonSerializerOptions)
	?? throw new Exception("Failed to deserialize directives");
var types = schema.GetProperty("types").Deserialize<ImmutableList<RawQueryType>>(jsonSerializerOptions)?
	.Select(raw => new QueryType(raw))
	.OrderBy(queryType => queryType.Name)
	.ToImmutableList()
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

connection?.Dispose();
