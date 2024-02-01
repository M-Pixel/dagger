using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using DaggerSDK;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using IntegrationTests.TestData;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using static DaggerSDK.Dagger;
using static DaggerSDK.TestHelpers;

namespace IntegrationTests;

[NonParallelizable]
class CSharpDefaultClient
{
	[Test]
	public async Task ItShouldUseTheDefaultClientAndCloseConnectionOnCallToClose()
	{
		// Check if the connection is actually not set before calling an execution
		// We verify the lazy evaluation that way
		Assert.That(Client.Default.Context.Client, Is.Null);

		string standardOut = await Client.Default
			.Container()
			.From("alpine:3.16.2")
			.WithExec(["echo", "hello", "world"])
			.Stdout();

		Assert.That(standardOut, Is.EqualTo("hello world\n"));

		// Check if the connection is still up
		Assert.That(Client.Default.Context.Client, Is.Not.Null);

		Close();

		Assert.That(Client.Default.Context.Client, Is.Null);
	}

	[Test]
	public async Task ItShouldAutomaticallyCloseConnection()
	{
		// Check if the connection is actually not set before calling connection
		Assert.That(Client.Default.Context.Client, Is.Null);

		await Connection
		(
			async () =>
			{
				string standardOut = await Client.Default
					.Container()
					.From("alpine:3.16.2")
					.WithExec(["echo", "hello", "world"])
					.Stdout();

				Assert.That(standardOut, Is.EqualTo("hello world\n"));

				// Check if the connection is still up
				Assert.That(Client.Default.Context.Client, Is.Not.Null);
			}
		);

		Assert.That(Client.Default.Context.Client, Is.Null);
	}

	[Test]
	public async Task ItShouldAutomaticallyCloseConnectionWithConfig()
	{
		// Check if the connection is actually not set before calling connection
		Assert.That(Client.Default.Context.Client, Is.Null);

		await Connection
		(
			async () =>
			{
				string standardOut = await Client.Default
					.Container()
					.From("alpine:3.16.2")
					.WithExec(["echo", "hello", "world"])
					.Stdout();

				Assert.That(standardOut, Is.EqualTo("hello world\n"));
			},
			new ConnectionOptions(LogOutput: Console.OpenStandardError())
		);

		Assert.That(Client.Default.Context.Client, Is.Null);
	}

	[Test]
	public async Task ItShouldParseDaggerSessionPortAndDaggerSessionTokenCorrectly()
	{
		string? originalSessionToken = SessionToken;
		string? originalSessionPort = SessionPort;
		Environment.SetEnvironmentVariable("DAGGER_SESSION_TOKEN", "foo");
		Environment.SetEnvironmentVariable("DAGGER_SESSION_PORT", "1234");

		await Connect
		(
			async client =>
			{
				Assert.That(client.Context.Client, Is.Not.Null);
				var graphQLClient = (GraphQLHttpClient)await client.Context.Client!;
				AuthenticationHeaderValue? authorization = graphQLClient.HttpClient.DefaultRequestHeaders.Authorization;

				Assert.That(graphQLClient.Options.EndPoint?.Port, Is.EqualTo(1234));
				Assert.That(authorization, Is.EqualTo(new AuthenticationHeaderValue("Basic", "Zm9vOg==")));
			},
			new ConnectionOptions(LogOutput: Console.OpenStandardError())
		);

		Environment.SetEnvironmentVariable("DAGGER_SESSION_PORT", originalSessionPort);
		Environment.SetEnvironmentVariable("DAGGER_SESSION_TOKEN", originalSessionToken);
	}

	[Test]
	public async Task ConnectToLocalEngineAndExecuteASimpleQueryToMakeSureItDoesNotFail()
	{
		await Connect
		(
			async client =>
			{
				await client
					.Container()
					.From("alpine")
					.WithExec(["apk", "add", "curl"])
					.WithExec(["curl", "https://dagger.io/"])
					.Sync();
			}
		);
	}
}

class CSharpSDKConnect
{
	[Test]
	public async Task ItThrowsError()
	{
		await TestParallelConnect
		(
			async client =>
			{
				try
				{
					await client.Container().From("alpine").File("unknown_file").Contents();
				}
				catch (GraphQLRequestErrorException)
				{
					return;
				}
				Assert.Fail();
			}
		);
	}
}
