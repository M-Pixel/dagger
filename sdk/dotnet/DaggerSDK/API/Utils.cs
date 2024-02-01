using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using GraphQL;
using GraphQL.Client.Abstractions;

namespace DaggerSDK;

static class APIUtils
{
	/// <summary>
	/// Find QueryTree, convert them into GraphQl query then compute and return the result to the appropriate field.
	/// </summary>
	static void ComputeNestedQuery(this IReadOnlyList<Operation> query)
	{
		foreach (Operation queryTree in query)
			queryTree.Arguments?.PrimeLinkedListInParallel();
	}

	/// <summary>Convert the queryTree into a GraphQL query.</summary>
	internal static async Task BuildQuery(StringBuilder queryOut, IReadOnlyList<Operation> queryTree)
	{
		queryOut.Append('{');
		foreach (Operation operation in queryTree)
		{
			queryOut.Append(operation.Name);
			if (operation.Arguments != null)
			{
				queryOut.Append('(');
				await operation.Arguments.SerializeLinkedList(queryOut);
				queryOut.Append(')');
			}
			queryOut.Append('{');
		}
		queryOut[^1] = '}';
		for (int i = 1; i < queryTree.Count; ++i)
			queryOut.Append('}');
	}

	/// <summary>Convert querytree into a Graphql query then compute it.</summary>
	public static async Task<JsonElement> ComputeQuery(ImmutableList<Operation> queryTree, IGraphQLClient client)
	{
		queryTree.ComputeNestedQuery();
		StringBuilder queryStringBuilder = new();
		await BuildQuery(queryStringBuilder, queryTree);
		return await Compute(queryStringBuilder.ToString(), client);
	}

	/// <summary>Return a Graphql query result flattened.</summary>
	internal static JsonElement QueryFlatten(JsonElement response)
	{
		// Recursion break condition
		// If our response is not an object we assume we reached the value
		if (response.ValueKind != JsonValueKind.Object)
			return response;

		JsonElement.ObjectEnumerator enumerator = response.EnumerateObject();
		enumerator.MoveNext();
		int childCount = 1;
		JsonElement inner = enumerator.Current.Value;

		if (enumerator.MoveNext())
			++childCount;

		if (childCount != 1)
			// Dagger is currently expecting to only return one value
			// If the response is nested in a way were more than one object is nested inside throw an error
			throw new TooManyNestedObjectsException
			(
				"Too many nested objects inside graphql response",
				new TooManyNestedObjectsExceptionOptions(response)
			);

		return QueryFlatten(inner);
	}

	/// <summary>Send a GraphQL document to the server.</summary>
	/// <returns>A flattened result.</returns>
	/// <exception cref="ExecErrorException"></exception>
	/// <exception cref="GraphQLRequestErrorException"></exception>
	static async Task<JsonElement> Compute(string query, IGraphQLClient client)
	{
		GraphQLResponse<JsonDocument> response = await client.SendQueryAsync<JsonDocument>(query);
		if (response.Errors is { Length: > 0 } errors)
		{
			string message = errors[0].Message;
			Map? extensions = errors[0].Extensions;

			if (extensions?["_type"] is "EXEC_ERROR")
				throw new ExecErrorException
				(
					message,
					new ExecErrorExceptionOptions
					(
						Command: extensions["cmd"] as string[] ?? [],
						ExitCode: extensions["exitCode"] as int? ?? -1,
						Stdout: extensions["stdout"] as string ?? "",
						Stderr: extensions["stderr"] as string ?? ""
					)
				);

			throw new GraphQLRequestErrorException(message, new GraphQLRequestErrorExceptionOptions(response, query));
		}

		return QueryFlatten(response.Data.RootElement);
	}
}
