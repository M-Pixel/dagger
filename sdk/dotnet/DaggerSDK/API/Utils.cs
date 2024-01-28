using System.Collections;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using GraphQL;
using GraphQL.Client.Abstractions;

namespace DaggerSDK;

static class APIUtils
{
	/// <summary>Format argument into GraphQL query format.</summary>
	static async Task BuildArguments(StringBuilder queryOut, OperationArgument[] operationArguments)
	{
		async Task serializeValue(ParameterSerialization qlType, object value)
		{
			switch (qlType)
			{
				case ParameterSerialization.String:
					// Use JSON serializer to accomplish proper escaping for special characters
					queryOut.Append(JsonSerializer.Serialize(value));
					break;
				case ParameterSerialization.Object:
					queryOut.Append('{');
					var members = (OperationArgument[])value;
					int index = 0;
					next:
					queryOut.Append(members[index].Name);
					queryOut.Append(':');
					await serializeValueMaybeArray(members, index);
					if (index != members.Length)
					{
						queryOut.Append(',');
						goto next;
					}
					queryOut.Append('}');
					break;
				case ParameterSerialization.Enum:
					queryOut.Append(value);
					break;
				case ParameterSerialization.Reference:
					queryOut.Append('"');
					queryOut.Append(await (Task<string>)value);
					queryOut.Append('"');
					break;
			}
		}

		async Task serializeValueMaybeArray(OperationArgument[] arguments, int argumentIndex)
		{
			if (arguments[argumentIndex].IsArray)
			{
				queryOut.Append('[');
				ParameterSerialization parameterSerialization = arguments[argumentIndex].HowToSerialize;

				foreach (object element in (IEnumerable)arguments[argumentIndex].Value!)
				{
					await serializeValue(parameterSerialization, element);
					queryOut.Append(',');
				}

				queryOut[^1] = ']'; // Replace final trailing comma
			}
			else
				await serializeValue(arguments[argumentIndex].HowToSerialize, arguments[argumentIndex].Value!);
		}

		for (int index = 0; index < operationArguments.Length; ++index)
		{
			if (operationArguments[index].Value != null)
			{
				queryOut.Append(operationArguments[index].Name);
				queryOut.Append(':');
				await serializeValueMaybeArray(operationArguments, index);
			}
		}
	}

	/// <summary>
	/// Find QueryTree, convert them into GraphQl query then compute and return the result to the appropriate field.
	/// </summary>
	static void ComputeNestedQuery(IReadOnlyList<Operation> query, IGraphQLClient queryClient)
	{
		IList<Task<string>> computeMany(IEnumerable<BaseClient> clientObjects)
		{
			// Allocate different list implementation depending on whether we know total count ahead of time
			IList<Task<string>> result = clientObjects is ICollection<BaseClient> list
				? new List<Task<string>>(list.Count)
				: ImmutableList.CreateBuilder<Task<string>>();
			foreach (BaseClient clientObject in clientObjects)
				result.Add(compute(clientObject));

			return result;
		}
		async Task<string> compute(BaseClient clientObject)
		{
			var jsonResult = await ComputeQuery
			(
				clientObject.QueryTree.Add("id"),
				await clientObject.Context.Connection()
			);
			return jsonResult.Deserialize<string>()!;
		}

		foreach (Operation queryTree in query.Where(tree => tree.Arguments.Length != 0))
		{
			for (int argumentIndex = 0; argumentIndex < queryTree.Arguments.Length; argumentIndex++)
			{
				if (queryTree.Arguments[argumentIndex].HowToSerialize == ParameterSerialization.Reference)
				{
					if (queryTree.Arguments[argumentIndex].IsArray)
					{
						lock (queryTree.Arguments)
						{
							switch (queryTree.Arguments[argumentIndex].Value)
							{
								case Task<IList<string>>:
									// Already computed, or already started by another thread
									continue;
								case IEnumerable<BaseClient> clientObjects:
									queryTree.Arguments[argumentIndex].Value = computeMany(clientObjects);
									break;
								default:
									throw new Exception
									(
										$"{queryTree.Arguments[argumentIndex].Value?.GetType().Name} " +
										$"{queryTree.Arguments[argumentIndex].Name} marked IsArray"
									);
							}
						}
					}
					else
					{
						lock (queryTree.Arguments)
						{
							if (queryTree.Arguments[argumentIndex].Value is not Task<string>)
							{
								Task<string> newTask = compute((BaseClient)queryTree.Arguments[argumentIndex].Value!);
								queryTree.Arguments[argumentIndex].Value = newTask;
							}
						}
					}
				}
			}
		}
	}

	/// <summary>Convert the queryTree into a GraphQL query.</summary>
	static async Task BuildQuery(StringBuilder queryOut, IReadOnlyList<Operation> queryTree)
	{
		queryOut.Append('{');
		foreach (Operation operation in queryTree)
		{
			queryOut.Append(operation.Name);
			if (operation.Arguments.Any(argument => argument.Value != null))
			{
				queryOut.Append('(');
				await BuildArguments(queryOut, operation.Arguments);
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
		ComputeNestedQuery(queryTree, client);
		StringBuilder queryStringBuilder = new();
		await BuildQuery(queryStringBuilder, queryTree);
		return await Compute(queryStringBuilder.ToString(), client);
	}

	/// <summary>Return a Graphql query result flattened.</summary>
	static JsonElement QueryFlatten(JsonElement response)
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
