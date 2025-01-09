using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dagger.Generated.ModuleTest;
using GraphQL;

namespace Dagger.Thunk;

class Invocation
{
	private readonly Assembly _moduleAssembly;
	JsonSerializerOptions? _serializerOptions;

	public Invocation(Assembly moduleAssembly)
	{
		_moduleAssembly = moduleAssembly;
	}

	public async Task Run(FunctionCall functionCall, string parentName, string moduleName)
	{
		Console.Write(parentName);
		Console.Write('.');
		Task<string> functionNameTask = functionCall.Name();
		Task<JSON> parentJsonTask = functionCall.Parent();

		Type? parentType = _moduleAssembly.ExportedTypes.FirstOrDefault(type => type.Name == parentName);
		if (parentType == null && parentName != moduleName && parentName != $"{moduleName}Static")
		{
			throw new Exception($"Missing type {parentName}");
		}

		// If function is static, there is no object to deserialize.
		Task<object?> parentTask = parentType == null || parentType.IsAbstract
			? Task.FromResult<object?>(null)
			: parentJsonTask.ContinueWith<object?>
			(
				jsonTask => JsonSerializer.Deserialize(jsonTask.Result.Value, parentType, SerializerOptions)
			);

		Task<MethodBase> functionTask = functionNameTask
			.ContinueWith<MethodBase>
			(
				nameTask =>
				{
					string functionName = nameTask.Result;
					Console.Write(functionName);
					Console.WriteLine("(...)");

					if (parentType == null || parentType.IsAbstract)
					{
						if (parentType != null)
						{
							if (functionName == "")
								return typeof(object).GetConstructor([])!;
							// If parent type is not null, I got here because parent type is abstract.  The only way for
							// methods from an abstract class to be registered is if it's the eponymous class and is
							// static (if it wasn't on the eponymous class, it would be mapped to non-existent
							// ModuleName or non-existent ModuleNameStatic).  There's a slightly higher chance that the
							// method is on that class, but it might  not be.  Try this class first.
							MemberInfo[] eponymousOption = parentType
								.GetMember(functionName, MemberTypes.Method, BindingFlags.Public|BindingFlags.Static);
							if (eponymousOption.Length > 0)
								return (MethodInfo)eponymousOption[0];
							// Proceed to look for the static method on other classes.
						}

						foreach (Type type in _moduleAssembly.ExportedTypes)
						{
							MemberInfo[] matchingMembers = type
								.GetMember(functionName, MemberTypes.Method, BindingFlags.Public|BindingFlags.Static);
							if (matchingMembers.Length != 0)
								return (MethodInfo)matchingMembers[0];
						}

						throw new Exception($"No type has static method {functionName}");
					}

					// At this point, I need to assume that it's an instance method.  If it were a static method, either
					// it would be mapped to non-existent class ModuleName, or to non-existent class ModuleNameStatic,
					// or to existing class ModuleName which is itself static, all three of which are caught by the
					// above condition.

					if (functionName == "")
						return (ConstructorInfo)parentType.GetMember(".ctor")[0];

					return (MethodInfo)parentType
						.GetMember(functionName, MemberTypes.Method, BindingFlags.Public|BindingFlags.Instance)[0];
				}
			);
		Task<object?[]> argumentsTask = functionCall.InputArgs()
			.ContinueWith(prior => ResolveFunctionArguments(prior.Result, functionTask)).Unwrap();

		var function = await functionTask;
		var parent = await parentTask;
		var arguments = await argumentsTask;
		try
		{
			object? returnValue = function.Invoke(parent, arguments);

			if (returnValue?.GetType() == typeof(Task))
			{
				await (Task)returnValue;
				returnValue = null;
			}
			else if (returnValue is ValueTask valueTask)
			{
				await valueTask;
				returnValue = null;
			}

			if (returnValue is null)
				await functionCall.ReturnValue(new JSON(""));
			else
			{
				string json = JsonSerializer.Serialize(returnValue, SerializerOptions);
				Console.WriteLine(json);
				await functionCall.ReturnValue(new JSON(json));
			}
		}
		catch (AggregateException aggregateException)
		{
			foreach (Exception innerException in TraverseAggregateExceptions(aggregateException))
			{
				switch (innerException)
				{
					case ExecErrorException execException:
						Console.Error.Write("Command failed (");
						Console.Error.Write(execException.ExitCode);
						Console.Error.Write("): ");
						Console.Error.Write(string.Join('\u241f', execException.Command));
						Console.Error.WriteLine("---------- stdout ----------");
						Console.Error.WriteLine(execException.Stdout);
						Console.Error.WriteLine("---------- stderr ----------");
						Console.Error.WriteLine(execException.Stderr);
						Environment.Exit((int)execException.Code);
						return;

					case GraphQLRequestErrorException requestException:
						Console.Error.WriteLine("Query failed.");
						Console.Error.WriteLine("---------- context ----------");
						Console.Error.WriteLine(requestException.RequestContext);
						Console.Error.WriteLine("---------- response ----------");
						foreach (GraphQLError graphQLError in requestException.Response.Errors)
						{
							Console.Error.WriteLine(graphQLError.Message);
							if (graphQLError.Path != null)
							{
								Console.Error.Write("\tPath: ");
								Console.Error.WriteLine(string.Join(' ', graphQLError.Path.Select(o => o.ToString())));
							}
							if (graphQLError.Extensions != null)
								foreach (KeyValuePair<string,object> extension in graphQLError.Extensions)
									Console.Error.WriteLine($"\t{extension.Key}: {extension.Value}");
						}
						Environment.Exit((int)requestException.Code);
						return;
				}
			}

			throw;
		}
	}

	static IEnumerable<Exception> TraverseAggregateExceptions(AggregateException aggregateException)
	{
		foreach (Exception innerException in aggregateException.InnerExceptions)
		{
			if (innerException is AggregateException innerAggregate)
				foreach (Exception innerInnerException in TraverseAggregateExceptions(innerAggregate))
					yield return innerInnerException;
			else
				yield return innerException;
		}
	}

	Task<object?[]> ResolveFunctionArguments
	(
		IReadOnlyList<FunctionCallArgValue> daggerArguments,
		Task<MethodBase> functionTask
	)
	{
		var result = new Task<object?>[daggerArguments.Count];
		Task<ParameterInfo[]> parametersTask = functionTask.ContinueWith(priorTask => priorTask.Result.GetParameters());

		// TODO: Do a batch query to Dagger, reduce total number of queries (and make it easier to do that with the Client!)
		for (int index = 0; index < daggerArguments.Count; ++index)
		{
			FunctionCallArgValue daggerArgument = daggerArguments[index];
			Task<string> nameTask = daggerArgument.Name();
			Task<JSON> valueTask = daggerArgument.Value();
			result[index] = Task.WhenAll(nameTask, valueTask, parametersTask).ContinueWith
			(
				_ =>
				{
					string name = nameTask.Result;
					ParameterInfo parameterInfo = parametersTask.Result.First(parameter => parameter.Name == name);;
					return JsonSerializer.Deserialize
					(
						valueTask.Result.Value,
						parameterInfo.ParameterType,
						SerializerOptions
					);
				}
			);
		}
		return Task.WhenAll(result);
	}

	JsonSerializerOptions SerializerOptions => _serializerOptions ??= new()
	{
		Converters =
		{
			new SelfSerializableConverterFactory(),
			new JsonStringEnumConverter(),
			new TaskConverterFactory(),
			new ValueTaskConverterFactory()
		},
		IncludeFields = true,
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
	};
}