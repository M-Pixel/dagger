using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
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
		Task<string> functionNameTask = functionCall.Name();
		Task<JSON> parentJsonTask = functionCall.Parent();

		Type? parentType = _moduleAssembly.ExportedTypes.FirstOrDefault(type => type.Name == parentName);
		if (parentType == null && parentName != moduleName && parentName != $"{moduleName}Static")
		{
			throw new Exception($"Missing type {parentName}");
		}

		// If function is static, there is no object to deserialize.
		Task<object?> parentTask = parentType == null || parentType.IsAbstract
			? Task.FromResult<object?>(null) // TODO: Support (de)serialization of static fields
			: parentJsonTask.ContinueWith<object?>
			(
				jsonTask => JsonSerializer.Deserialize(jsonTask.Result.Value, parentType, SerializerOptions)
			);

		Task<FunctionSearchResult> functionTask = functionNameTask.ContinueWith
		(
			nameTask =>
			{
				string functionName = nameTask.Result;

				if (parentType == null || parentType.IsAbstract)
				{
					FunctionSearchResult result;
					BindingFlags staticBinding = BindingFlags.Public | BindingFlags.Static;
					if (parentType != null)
					{
						if (functionName == "")
							return new FunctionSearchResult((_, _) => new object(), parentType, []);
						// If parent type is not null, I got here because parent type is abstract.  The only way for
						// methods from an abstract class to be registered is if it's the eponymous class and is static
						// (if it wasn't on the eponymous class, it would be mapped to non-existent ModuleName or
						// non-existent ModuleNameStatic).  There's a slightly higher chance that the method is on that
						// class, but it might  not be.  Try this class first.
						if (TryFindMethod(parentType, functionName, staticBinding, out result))
							return result;
						// Proceed to look for the static method on other classes.
					}

					foreach (Type type in _moduleAssembly.ExportedTypes)
						if (type != parentType && TryFindMethod(type, functionName, staticBinding, out result))
							return result;

					throw new Exception($"No type has static method {functionName}");
				}

				// At this point, I need to assume that it's an instance method.  If it were a static method, either it
				// would be mapped to non-existent class ModuleName, or to non-existent class ModuleNameStatic, or to
				// existing class ModuleName which is itself static, all three of which are caught by the above
				// condition.

				if (functionName == "")
				{
					MemberInfo[] constructor = parentType.GetMember(".ctor");
					if (constructor.Length == 0)
						throw new Exception($"No .ctor in {parentName}.");
					var constructorInfo = (ConstructorInfo)constructor[0];
					return new FunctionSearchResult
					(
						(_, arguments) => constructorInfo.Invoke(null, arguments),
						parentType,
						ParameterIdentity.Convert(constructorInfo.GetParameters())
					);
				}

				var instanceMethodFlags = BindingFlags.Public | BindingFlags.Instance;
				if (TryFindMethod(parentType, functionName, instanceMethodFlags, out var callable))
					return callable;

				throw new Exception($"Type {parentType} has no function {functionName}");
			}
		);
		Task<object?[]> argumentsTask = functionCall.InputArgs()
			.ContinueWith(prior => ResolveFunctionArguments(prior.Result, functionTask)).Unwrap();

		var function = await functionTask;
		var parent = await parentTask;
		var arguments = await argumentsTask;
		try
		{
			object? returnValue = function.Callable(parent, arguments);

			if (function.ReturnType == typeof(Task))
			{
				await (Task)returnValue!;
				returnValue = null;
			}
			else if (returnValue is ValueTask valueTask)
			{
				await valueTask;
				returnValue = null;
			}

			if (returnValue is null)
				await functionCall.ReturnValue(new JSON("null"));
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
				Console.Error.WriteLine($"Module threw {innerException.GetType().FullName}: {innerException.Message}");
				LogExceptionProperties(innerException);
				Console.Error.WriteLine(innerException.StackTrace);
			}

			Environment.Exit(1);
		}
		catch (Exception exception)
		{
			Console.Error.WriteLine($"Module threw {exception.GetType().FullName}: {exception.Message}");
			LogExceptionProperties(exception);
			Console.Error.WriteLine(exception.StackTrace);

			Environment.Exit(1);
		}
	}

	/// <summary>
	/// Checks if the given type has overridden ToString directly (not inherited from parent)
	/// </summary>
	private static bool HasCustomToStringMethod(Type type)
	{
		var method = type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
		return method != null && method.DeclaringType == type;
	}

	/// <summary>
	/// Determines if a value should be printed directly or recursively processed
	/// </summary>
	private static bool ShouldPrintDirectly(object? value)
	{
		if (value == null)
			return true;

		Type valueType = value.GetType();
		return value is string || valueType.IsPrimitive || HasCustomToStringMethod(valueType);
	}

	/// <summary>
	/// Recursively prints a property value with proper indentation based on depth
	/// </summary>
	private static void PrintPropertyValue(string propertyName, object? value, int depth, HashSet<object>? visited = null)
	{
		visited ??= new HashSet<object>();
		string indent = new('\t', depth);

		// Handle null values
		if (value == null)
		{
			Console.Error.WriteLine($"{indent}{propertyName}: null");
			return;
		}

		// If the value can be printed directly, do so now
		if (ShouldPrintDirectly(value))
		{
			Console.Error.WriteLine($"{indent}{propertyName}: {value}");
			return;
		}

		// Prevent infinite recursion
		if (value is not ValueType && !visited.Add(value))
		{
			Console.Error.WriteLine($"{indent}{propertyName}: *");
			return;
		}

		// Handle dictionary types specifically
		if (value is IDictionary dictionary)
		{
			Console.Error.WriteLine($"{indent}{propertyName}:");
			foreach (DictionaryEntry entry in dictionary)
			{
				string keyStr = entry.Key.ToString() ?? "null";
				PrintPropertyValue(keyStr, entry.Value, depth + 1, visited);
			}
			return;
		}

		// Handle general collections
		if (value is IEnumerable collection and not string)
		{
			Console.Error.WriteLine($"{indent}{propertyName}:");
			int i = 0;
			foreach (var item in collection)
			{
				PrintPropertyValue($"[{i++}]", item, depth + 1, visited);
			}
			return;
		}

		// By this point, the value is a complex object but not a collection
		// Print its properties recursively
		PropertyInfo[] valueProperties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
		if (valueProperties.Length > 0)
		{
			Console.Error.WriteLine($"{indent}{propertyName}:");
			foreach (var valueProperty in valueProperties)
			{
				try
				{
					object? propValue = valueProperty.GetValue(value);
					PrintPropertyValue(valueProperty.Name, propValue, depth + 1, visited);
				}
				catch (Exception)
				{
					// Ignore property access failures
				}
			}
			return;
		}

		// For objects with no properties, just print the value
		Console.Error.WriteLine($"{indent}{propertyName}: {value}");
	}

	private static void LogExceptionProperties(Exception exception)
	{
		// Exceptions often include useful information in additional properties.
		Type exceptionType = exception.GetType();
		PropertyInfo[] properties = exceptionType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

		foreach (PropertyInfo property in properties)
		{
			try
			{
				if (property.Name is "Message" or "StackTrace" or "InnerExceptions")
					continue;

				object? value = property.GetValue(exception);
				PrintPropertyValue(property.Name, value, 1);
			}
			catch (Exception)
			{
				// Ignore exception property access failures
			}
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

	async Task<object?[]> ResolveFunctionArguments
	(
		IReadOnlyList<FunctionCallArgValue> daggerArguments,
		Task<FunctionSearchResult> functionTask
	)
	{
		// Dagger doesn't (always) deliver parameters in the same order that they were declared in introspection.
		var unsortedResultTasks = new Task<ParameterValue>[daggerArguments.Count];

		// TODO: Do a batch query to Dagger, reduce total number of queries (and make it easier to do that with the Client!)
		for (int index = 0; index < daggerArguments.Count; ++index)
		{
			FunctionCallArgValue daggerArgument = daggerArguments[index];
			Task<string> nameTask = daggerArgument.Name();
			Task<JSON> valueTask = daggerArgument.Value();
			unsortedResultTasks[index] = Task.WhenAll(nameTask, valueTask, functionTask).ContinueWith
			(
				_ =>
				{
					string name = string.Intern(nameTask.Result);
					ParameterIdentity parameter =
						functionTask.Result.Parameters.First(parameter => parameter.Name == name);
					return new ParameterValue
					(
						name,
						JsonSerializer.Deserialize
						(
							valueTask.Result.Value,
							parameter.Type,
							SerializerOptions
						)
					);
				}
			);
		}
		ParameterValue[] unsortedResults = await Task.WhenAll(unsortedResultTasks);
		return functionTask.Result.Parameters
			.Select
			(
				parameter =>
				{
					ParameterValue result = unsortedResults.FirstOrDefault(result => result.Name == parameter.Name);
					return result.Name == null ? parameter.DefaultValue : result.Value;
				}
			)
			.ToArray();
	}

	private static bool TryFindMethod
	(
		Type parentType,
		string name,
		BindingFlags bindingFlags,
		out FunctionSearchResult result
	)
	{
		MemberInfo[] member = parentType.GetMember(name, MemberTypes.Method, bindingFlags);
		if (member.Length > 0)
		{
			var methodInfo = (MethodInfo)member[0];
			result = new FunctionSearchResult
			(
				(self, arguments) => methodInfo.Invoke(self, arguments),
				methodInfo.ReturnType,
				ParameterIdentity.Convert(methodInfo.GetParameters())
			);
			return true;
		}

		if (name.StartsWith("With", StringComparison.Ordinal))
		{
			member = parentType.GetMember("set_" + name[4..], MemberTypes.Method, bindingFlags|BindingFlags.NonPublic);
			if (member.Length > 0)
			{
				var methodInfo = (MethodInfo)member[0];
				result = new FunctionSearchResult
				(
					(self, arguments) => methodInfo.Invoke(self, arguments),
					parentType,
					ParameterIdentity.Convert(methodInfo.GetParameters())
				);
				return true;
			}

			member = parentType.GetMember(name[4..], MemberTypes.Field, bindingFlags);
			if (member.Length > 0)
			{
				var fieldInfo = (FieldInfo)member[0];
				result = new FunctionSearchResult
				(
					(self, arguments) =>
					{
						fieldInfo.SetValue(self, arguments[0]);
						return self;
					},
					parentType,
					// DefaultValue is supplied only because it is required; setter parameters are never optional.
					[new ParameterIdentity("value", fieldInfo.FieldType, DefaultValue: null)]
				);
				return true;
			}
		}

		result = default;
		return false;
	}

	JsonSerializerOptions SerializerOptions => _serializerOptions ??= new()
	{
		Converters =
		{
			new SelfSerializableConverterFactory(),
			new JsonStringEnumConverter(),
			new TaskConverterFactory(),
			new ValueTaskConverterFactory(),
			new ConstructorlessConverterFactory(_moduleAssembly)
		},
		IncludeFields = true,
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver
		{
			Modifiers =
			{
				ConstructorlessConverter.TypeInfoModifier,
				PrivateMemberSerialization.TypeInfoModifierFactory(_moduleAssembly)
			}
		}
	};

	private delegate object? Callable(object? self, object?[] parameters);

	private readonly record struct ParameterIdentity(string Name, Type Type, object? DefaultValue)
	{
		public static ImmutableArray<ParameterIdentity> Convert(IEnumerable<ParameterInfo> infos) =>
			[..infos.Select(static info => new ParameterIdentity(info.Name!, info.ParameterType, info.DefaultValue))];
	}

	private readonly record struct ParameterValue(string Name, object? Value);

	private readonly record struct FunctionSearchResult
	(
		Callable Callable,
		Type ReturnType,
		ImmutableArray<ParameterIdentity> Parameters
	);
}
