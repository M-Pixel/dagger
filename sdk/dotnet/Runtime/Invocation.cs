using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;

namespace Dagger.Runtime;

class Invocation
{
	private readonly ImmutableArray<Type> _allTypes;

	public Invocation(Assembly moduleAssembly)
	{
		_allTypes = [..moduleAssembly.ExportedTypes];
	}

	public async Task Run(FunctionCall functionCall, string parentName, string moduleName)
	{
		Console.WriteLine(parentName);
		Task<JSON> parentJsonTask = functionCall.Parent();
		Task<string> functionNameTask = functionCall.Name();

		Type? parentType = _allTypes.FirstOrDefault(type => type.Name == parentName);
		if (parentType == null && parentName != moduleName && parentName != $"{moduleName}Static")
		{
			throw new Exception($"Missing type {parentName}");
		}

		Task<MethodInfo> functionTask = functionNameTask
			.ContinueWith
			(
				nameTask =>
				{
					string functionName = nameTask.Result;

					if (parentType == null)
					{
						foreach (Type type in _allTypes)
						{
							MemberInfo[] matchingMembers = type
								.GetMember(functionName, MemberTypes.Method, BindingFlags.Public|BindingFlags.Static);
							if (matchingMembers.Length != 0)
								return (MethodInfo)matchingMembers[0];
						}

						throw new Exception($"No type has static method {functionName}");
					}
					return (MethodInfo)parentType.GetMember(functionName, MemberTypes.Method, BindingFlags.Public)[0];
				}
			);
		Task<object?[]> functionArguments = functionCall.InputArgs()
			.ContinueWith(prior => ResolveFunctionArguments(prior.Result, functionTask)).Unwrap();

		object? returnValue = (await functionTask).Invoke(await parentJsonTask, await functionArguments);

		if (returnValue is ValueTask voidValueTask)
		{
			await voidValueTask;
			returnValue = null;
		}
		if (returnValue is Task task)
		{
			await task;
			Type taskType = task.GetType();
			returnValue = taskType.IsGenericType
				? taskType.GetProperty(nameof(Task<object>.Result))!.GetValue(task)
				: null;
		}
		if (returnValue is null)
		{
			await functionCall.ReturnValue(new JSON(""));
		}
		else
		{
			await functionCall.ReturnValue(new JSON(JsonSerializer.Serialize(returnValue)));
		}
	}

	Task<object?[]> ResolveFunctionArguments
	(
		IReadOnlyList<FunctionCallArgValue> daggerArguments,
		Task<MethodInfo> functionTask
	)
	{
		var result = new Task<object?>[daggerArguments.Count];
		Task<ParameterInfo[]> parametersTask = functionTask.ContinueWith(priorTask => priorTask.Result.GetParameters());

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
					ParameterInfo parameterInfo = parametersTask.Result.First(parameter => parameter.Name == name);
					Type type = parameterInfo.ParameterType;
					string valueJson = valueTask.Result.Value;
					return Deserializer.Deserialize(type, valueJson);
				}
			);
		}
		return Task.WhenAll(result);
	}
}