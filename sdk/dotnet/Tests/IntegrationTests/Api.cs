using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using static Dagger.APIUtils;
using static Dagger.IntegrationTests.TestHelpers;

namespace Dagger.IntegrationTests;

class CSharpApi
{
	[Test]
	public async Task BuildCorrectlyAQueryAithOneArgument()
	{
		Container tree = new Query().Container().From("alpine:3.16.2");

		StringBuilder queryBuilder = new();
		await BuildQuery(queryBuilder, tree.QueryTree);
		Assert.That(queryBuilder.ToString(), Is.EqualTo("{container{from(address:\"alpine:3.16.2\")}}"));
	}

	[Test]
	public async Task BuildCorrectlyAQueryWithDifferentArgsType()
	{
		GitRepository tree2 = new Query().Git("fake_url", keepGitDir: true);

		StringBuilder queryBuilder = new();
		await BuildQuery(queryBuilder, tree2.QueryTree);
		Assert.That(queryBuilder.ToString(), Is.EqualTo("{git(keepGitDir:true,url:\"fake_url\")}"));

		queryBuilder.Clear();
		await BuildQuery
		(
			queryBuilder,
			[
				new Operation
				(
					"test_types",
					new OperationArgument
					(
						"id",
						EnumOperationArgumentValue.Create(1),
					new OperationArgument
					(
						"platform",
						ArrayOperationArgumentValue.Create
						(
							["string", "string2"],
							element => new StringOperationArgumentValue(element)
						),
					new OperationArgument
					(
						"boolean",
						EnumOperationArgumentValue.Create(true),
					new OperationArgument
					(
						"object",
						new ObjectOperationArgumentValue
						(
							new OperationArgument("member", EnumOperationArgumentValue.Create(CacheSharingMode.LOCKED))
						)
					))))
				)
			]
		);
		Assert.That
		(
			queryBuilder.ToString(),
			Is.EqualTo("{test_types(id:1,platform:[\"string\",\"string2\"],boolean:true,object:{member:LOCKED})}")
		);
	}

	[Test]
	public async Task BuildOneQueryWithMultipleArguments()
	{
		Container tree = new Query()
			.Container()
			.From("alpine:3.16.2")
			.WithExec(["apk", "add", "curl"]);

		StringBuilder queryBuilder = new();
		await BuildQuery(queryBuilder, tree.QueryTree);
		Assert.That
		(
			queryBuilder.ToString(),
			Is.EqualTo("{container{from(address:\"alpine:3.16.2\"){withExec(args:[\"apk\",\"add\",\"curl\"])}}}")
		);
	}

	[Test]
	public async Task BuildAQueryBySplittingIt()
	{
		Container image = new Query().Container().From("alpine:3.16.2");
		Container pkg = image.WithExec(["echo", "foo bar"]);

		StringBuilder queryBuilder = new();
		await BuildQuery(queryBuilder, pkg.QueryTree);

		Assert.That(queryBuilder.ToString(),
			Is.EqualTo("{container{from(address:\"alpine:3.16.2\"){withExec(args:[\"echo\",\"foo bar\"])}}}"));
	}

	[Test]
	public async Task PassAClientWithAnExplicitIdAsAParameter()
	{
		await TestParallelConnect
		(
			async client =>
			{
				ContainerID containerId = await client
					.Container()
					.From("alpine:3.16.2")
					.WithExec(["apk", "add", "yarn"])
					.Id();

				string image = await client
					.LoadContainerFromID(containerId)
					.WithMountedCache("/root/.cache", client.CacheVolume("cache_key"))
					.WithExec(["echo", "foo bar"])
					.Stdout();

				Assert.That(image, Is.EqualTo("foo bar\n"));
			}
		);
	}

	[Test]
	public async Task PassACacheVolumeWithAnImplicitIdAsAParameter()
	{
		await TestParallelConnect
		(
			async client =>
			{
				CacheVolume cacheVolume = client.CacheVolume("cache_key");

				string image = await client
					.Container()
					.From("alpine:3.16.2")
					.WithExec(["apk", "add", "yarn"])
					.WithMountedCache("/root/.cache", cacheVolume)
					.WithExec(["echo", "foo bar"])
					.Stdout();

				Assert.That(image, Is.EqualTo("foo bar\n"));
			}
		);
	}

	[Test]
	public async Task BuildAQueryWithPositionnalAndOptionalsArguments()
	{
		Container image = new Query()
			.Container()
			.From("alpine:3.16.2");
		Container pkg = image
			.WithExec(["apk", "add", "curl"], experimentalPrivilegedNesting: true);

		StringBuilder queryBuilder = new();
		await BuildQuery(queryBuilder, pkg.QueryTree);

		Assert.That(queryBuilder.ToString(),
			Is.EqualTo("{container{from(address:\"alpine:3.16.2\"){withExec(experimentalPrivilegedNesting:true,args:[\"apk\",\"add\",\"curl\"])}}}"));
	}

	[Test]
	public async Task TestFieldImmutability()
	{
		Container image = new Query()
			.Container()
			.From("alpine:3.16.2");

		Container a = image
			.WithExec(["echo", "hello", "world"]);
		StringBuilder queryBuilder = new();
		await BuildQuery(queryBuilder, a.QueryTree);
		Assert.That
		(
			queryBuilder.ToString(),
			Is.EqualTo("{container{from(address:\"alpine:3.16.2\"){withExec(args:[\"echo\",\"hello\",\"world\"])}}}")
		);

		Container b = image.WithExec(["echo", "foo", "bar"]);
		queryBuilder.Clear();
		await BuildQuery(queryBuilder, b.QueryTree);
		Assert.That
		(
			queryBuilder.ToString(),
			Is.EqualTo("{container{from(address:\"alpine:3.16.2\"){withExec(args:[\"echo\",\"foo\",\"bar\"])}}}")
		);
	}

	[Test]
	public async Task TestAwaitedFieldImmutability()
	{
		await TestParallelConnect
		(
			async client =>
			{
				Container image = client
					.Container()
					.From("alpine:3.16.2")
					.WithExec(["echo", "hello", "world"]);

				string a = await image
					.WithExec(["echo", "foobar"])
					.Stdout();
				Assert.That(a, Is.EqualTo("foobar\n"));

				string b = await image
					.Stdout();
				Assert.That(b, Is.EqualTo("hello world\n"));
			}
		);
	}

	[Test]
	public async Task RecursivelySolveSubQueries()
	{
		await TestParallelConnect
		(
			async client =>
			{
				Directory image = client.GetDirectory().WithNewFile
				(
					"Dockerfile",
					"FROM alpine"
				);

				Container builder = client
					.Container()
					.Build(image)
					.WithWorkdir("/")
					.WithEntrypoint(["sh", "-c"])
					.WithExec(["echo htrshtrhrthrts > file.txt"])
					.WithExec(["cat file.txt"]);

				string copiedFile = await client
					.Container()
					.From("alpine:3.16.2")
					.WithWorkdir("/")
					.WithFile("/copied-file.txt", builder.File("/file.txt"))
					.WithEntrypoint(["sh", "-c"])
					.WithExec(["cat copied-file.txt"])
					.File("copied-file.txt")
					.Contents();

				Assert.That(copiedFile, Is.EqualTo("htrshtrhrthrts\n"));
			}
		);
	}

	[Test]
	public void ReturnAFlattenedGraphQLResponse()
	{
		var tree = JsonDocument.Parse
		(
			"""
			{
			  "container": {
				"from": {
				  "withExec": {
					"stdout":
					  "fetch https://dl-cdn.alpinelinux.org/alpine/v3.16/main/aarch64/APKINDEX.tar.gz"
				  }
				}
			  }
			}
			"""
		);

		Assert.That
		(
			QueryFlatten(tree.RootElement).Deserialize<string>(),
			Is.EqualTo("fetch https://dl-cdn.alpinelinux.org/alpine/v3.16/main/aarch64/APKINDEX.tar.gz")
		);
	}

	[Test]
	public void ReturnAErrorForGraphQLObjectNestedResponse()
	{
		var tree = JsonDocument.Parse
		(
			"""
			{
			  "container": {
				"from": "from"
			  },
			  "host": {
				"directory": "directory"
			  }
			}
			"""
		);

		Assert.Throws<TooManyNestedObjectsException>(() => QueryFlatten(tree.RootElement));
	}

	[Test]
	public async Task ReturnCustomExecError()
	{
		const string stdout = "STDOUT HERE";
		const string stderr = "STDERR HERE";
		string[] args = ["sh", "-c", "cat /testout >&1; cat /testerr >&2; exit 127"];

		await TestParallelConnect
		(
			async client =>
			{
				Container ctr = client
					.Container()
					.From("alpine:3.16.2")
					.WithDirectory(
						"/",
						client
							.GetDirectory()
							.WithNewFile("testout", stdout)
							.WithNewFile("testerr", stderr))
					.WithExec(args);

				try
				{
					await ctr.Sync();
				}
				catch (ExecErrorException exception)
				{
					Assert.That(exception.Message, Does.Contain("did not complete successfully"));
					Assert.That(exception.ExitCode, Is.EqualTo(127));
					Assert.That(exception.Stdout, Is.EqualTo(stdout));
					Assert.That(exception.Stderr, Is.EqualTo(stderr));
					Assert.That(exception.ToString(), Does.Contain(stdout));
					Assert.That(exception.ToString(), Does.Contain(stderr));
					Assert.That(exception.Message, Does.Not.Contain(stdout));
					Assert.That(exception.Message, Does.Not.Contain(stderr));
					return;
				}
				Assert.Fail();
			}
		);
	}

	[Test]
	public async Task SupportContainerSync()
	{
		await TestParallelConnect
		(
			async client =>
			{
				Container baseContainer = client
					.Container()
					.From("alpine:3.16.2");

				bool caught = false;
				try
				{
					await baseContainer.WithExec(["foobar"]).Sync();
				}
				catch (ExecErrorException)
				{
					caught = true;
				}
				Assert.That(caught, Is.True);

				string output = await (await baseContainer.WithExec(["echo", "foobaz"]).Sync()).Stdout();
				Assert.That(output, Is.EqualTo("foobaz\n"));
			}
		);
	}

	[Test]
	public async Task SupportChainableUtilsViaWith()
	{
		await TestParallelConnect
		(
			async client =>
			{
				await client
					.Container()
					.From("alpine:3.16.2")
					.WithEnv()
					.WithSecret("baz", client)
					.WithExec(["sh", "-c", "test $FOO = bar && test $TOKEN = baz"])
					.Sync();
			}
		);
	}

	[Test]
	public async Task ComputeEmptyStringValue()
	{
		await TestParallelConnect(async client =>
		{
			Container alpine = client
				.Container()
				.From("alpine:3.16.2")
				.WithEnvVariable("FOO", "");

			string output = await alpine.WithExec(["printenv", "FOO"]).Stdout();

			Assert.That(output, Is.EqualTo("\n"));
		});
	}


	[Test]
	public async Task ComputeNestedArrayOfArguments()
	{
		var platforms = new[]
		{
			("linux/amd64", "x86_64"),
			("linux/arm64", "aarch64")
		};

		await TestParallelConnect
		(
			async client =>
			{
				var seededPlatformVariants = new List<Container>();

				foreach ((string platformName, string unameOutput) in platforms)
				{
					Container container = client
						.Container(platform: new Platform(platformName))
						.From("alpine:3.16.2")
						.WithExec(["uname", "-m"]);

					string result = await container.Stdout();

					Assert.That(result.Trim(), Is.EqualTo(unameOutput));

					seededPlatformVariants.Add(container);
				}

				string exportId = $"./export-{Guid.NewGuid()}";

				string _ = await client
					.Container()
					.Export(exportId, platformVariants: seededPlatformVariants);

				System.IO.File.Delete(exportId);
			}
		);
	}

	[Test]
	public async Task HandleEnumeration()
	{
		await TestParallelConnect
		(
			async client =>
			{
				ImmutableArray<Port> ports = await client
					.Container()
					.From("alpine:3.16.2")
					.WithExposedPort(8000, protocol: NetworkProtocol.UDP)
					.ExposedPorts();

				Assert.That(await ports[0].Protocol(), Is.EqualTo(NetworkProtocol.UDP));
			}
		);
	}

	[Test]
	public async Task HandleListOfObjects()
	{
		await TestParallelConnect
		(
			async client =>
			{
				var container = client
					.Container()
					.From("alpine:3.16.2")
					.WithEnvVariable("FOO", "BAR")
					.WithEnvVariable("BAR", "BOOL");

				ImmutableArray<EnvVariable> environmentVariables = await container.EnvVariables();

				Assert.That(await environmentVariables[1].Name(), Is.EqualTo("FOO"));
				Assert.That(await environmentVariables[1].Value(), Is.EqualTo("BAR"));

				Assert.That(await environmentVariables[2].Name(), Is.EqualTo("BAR"));
				Assert.That(await environmentVariables[2].Value(), Is.EqualTo("BOOL"));
			}
		);
	}

	[Test]
	public async Task CheckConflictWithEnum()
	{
		await TestParallelConnect
		(
			async client =>
			{
				string? environmentVariable = await client
					.Container()
					.From("alpine:3.16.2")
					.WithEnvVariable("FOO", "TCP")
					.EnvVariable("FOO");

				Assert.That(environmentVariable, Is.EqualTo("TCP"));
			}
		);
	}
}

static class TestExtensionMethods
{
	public static Container WithEnv(this Container container) => container.WithEnvVariable("FOO", "bar");

	public static Container WithSecret(this Container container, string token, Query client)
		=> container.WithSecretVariable("TOKEN", client.SetSecret("TOKEN", token));
}