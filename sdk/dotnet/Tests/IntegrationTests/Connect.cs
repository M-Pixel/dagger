using NUnit.Framework;
using static Dagger.IntegrationTests.TestHelpers;

namespace Dagger.IntegrationTests;

[NonParallelizable]
class CSharpDefaultClient
{
	[Test]
	public async Task ItShouldUseTheDefaultClientAndCloseConnectionOnCallToClose()
	{
		// Check if the connection is actually not set before calling an execution
		// We verify the lazy evaluation that way
		Assert.That(Query.FromDefaultSession.Session.IsActive, Is.False);

		string standardOut = await Query.FromDefaultSession
			.Container()
			.From("alpine:3.16.2")
			.WithExec(["echo", "hello", "world"])
			.Stdout();

		Assert.That(standardOut, Is.EqualTo("hello world\n"));

		// Check if the connection is still up
		Assert.That(Query.FromDefaultSession.Session.IsActive, Is.True);

		Query.FromDefaultSession.Session.Close();

		Assert.That(Query.FromDefaultSession.Session.IsActive, Is.False);
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
					await client
						.Container()
						.From("alpine")
						.File("unknown_file")
						.Contents();
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
