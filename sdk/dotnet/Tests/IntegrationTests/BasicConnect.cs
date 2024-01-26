using System.Text.Json;
using DaggerSDK;
using GraphQL;
using GraphQL.Client.Abstractions;
using IntegrationTests.TestData;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace IntegrationTests;

public class BasicTests
{
    [Test]
    public async Task BasicConnectAsync()
    {
        string query = LaravelExample.RuntimeQuery;
        IGraphQLClient client = await new Context().Connection();
        GraphQLResponse<JsonDocument> response = await client.SendQueryAsync<JsonDocument>(query);
        Console.WriteLine(JsonConvert.SerializeObject(response.Data.ToString(), Formatting.Indented));
        Assert.That(response.Errors == null || response.Errors.Length == 0);
    }

    [Test]
    public async Task ContainerBuilder()
    {
        string? query = null;
        GraphQLClientMock mockClient = new()
        {
            SendQueryAsyncOverride = async (Type type, GraphQLRequest request, CancellationToken token) =>
            {
                query = request.Query;
                return new GraphQLResponse<object> { Data = JsonSerializer.Deserialize<JsonDocument>("{\"x\":\"12345\"}")! };
            }
        };
        Client client = new() { Context = new Context(new ContextConfiguration(mockClient)) };
        await LaravelExample.ContainerBuilder(client);
        Assert.That(query, Is.EqualTo(LaravelExample.RuntimeQuery.Replace("\n", "")));
    }
}
