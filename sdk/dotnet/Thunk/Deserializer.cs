using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dagger.Thunk;

// TODO: Instead of doing all of this reflection at runtime, when building a C# Dagger module, code-gen function for each object + method that directly deserialize
public class Deserializer
{
	public static IReadOnlyDictionary<Type, Func<JsonElement, Type, object>> ListStrategies =
		new Dictionary<Type, Func<JsonElement, Type, object>>
		{
			{ typeof(IEnumerable<>), ToEnumerable },
			{ typeof(ICollection<>), ToList },
			{ typeof(IList<>), ToList },
			{ typeof(IReadOnlyCollection<>), ToList },
			{ typeof(IReadOnlyList<>), ToList },
			{ typeof(IReadOnlySet<>), ToHashSet },
			{ typeof(ISet<>), ToHashSet },
			{ typeof(IImmutableList<>), ToImmutableArray },
			{ typeof(IImmutableSet<>), ToImmutableSet },
		};

	public static object? Deserialize(Type type, string valueJson)
	{
		if (type.IsPrimitive)
			return type.Name switch
			{
				"String" => valueJson,
				"Int32" => int.Parse(valueJson),
				"Bool" => bool.Parse(valueJson),
				_ => throw new NotSupportedException()
			};

		if (type.IsValueType)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				Type innerType = type.GenericTypeArguments[0];
				if (valueJson == "undefined")
					return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, [])!.Invoke([]);
				return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, [innerType])!
					.Invoke([Deserialize(innerType, valueJson)]);
			}
		}
		else if (valueJson == "undefined")
			return null;
		else if (TryAsList(type, valueJson, out object? list))
			return list;

		// Must be an object
		if (type == typeof(JsonObject))
			return JsonNode.Parse(valueJson)!.AsObject();

		var jsonDocument = JsonDocument.Parse(valueJson);
		if (type == typeof(JsonDocument))
			return jsonDocument;

		return DeserializeObject(type, jsonDocument.RootElement);
	}

	public static object DeserializeObject(Type type, JsonElement element)
	{
		// if (type.IsAssignableTo(typeof(BaseClient)))
		// {
		// }
		return element.Deserialize(type)!;
	}

	public static bool TryAsList(Type type, string valueJson, [NotNullWhen(true)] out object? result)
	{
		if (type == typeof(JsonArray))
		{
			result = JsonSerializer.Deserialize<JsonArray>(valueJson) ?? throw new ArgumentNullException();
			return true;
		}

		if
		(
			type.IsGenericType &&
			ListStrategies.TryGetValue(type.GetGenericTypeDefinition(), out Func<JsonElement, Type, object>? strategy)
		)
		{
			result = strategy(JsonDocument.Parse(valueJson).RootElement, type.GenericTypeArguments[0]);
			return true;
		}

		result = null;
		return false;
	}

	private static Func<JsonElement, object?> DeserializerForType(Type type)
	{
		if (type.IsPrimitive)
			return type.Name switch
			{
				"String" => element => element.GetString()!,
				"Int32" => element => element.GetInt32(),
				"Bool" => element => element.GetBoolean(),
				_ => throw new NotSupportedException()
			};

		if (type == typeof(JsonArray))
			return element => element.Deserialize<JsonArray>()!;

		if (type.IsGenericType)
		{
			if (type.IsValueType)
			{
				if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					Type innerType = type.GenericTypeArguments[0];
					ConstructorInfo nullConstructor =
						type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, [])!;
					ConstructorInfo valueConstructor =
						type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, [innerType])!;
					Func<JsonElement, object?> innerDeserializer = DeserializerForType(innerType);
					return element => element.ValueKind == JsonValueKind.Undefined
						? nullConstructor.Invoke([])
						: valueConstructor.Invoke([innerDeserializer(element)]);
				}
			}
			else if
			(
				ListStrategies.TryGetValue
				(
					type.GetGenericTypeDefinition(),
					out Func<JsonElement, Type, object>? strategy
				)
			)
			{
				Type innerType = type.GenericTypeArguments[0];
				return element => element.ValueKind == JsonValueKind.Undefined ? null : strategy(element, innerType);
			}
		}

		// Must be an object
		if (type == typeof(JsonObject))
			return element => element.Deserialize<JsonObject>()!;

		return element => DeserializeObject(type, element);
	}

	private static object ToEnumerable(JsonElement json, Type innerType)
	{
		// TODO: avoid converting JSON from UTF8 byte array to string only to convert it back again
		IEnumerable<object?> untypedEnumerable = json.EnumerateArray().Select(DeserializerForType(innerType));
		return innerType == typeof(object)
			? untypedEnumerable
			: typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))!.MakeGenericMethod(innerType)
				.Invoke(null, [untypedEnumerable])!;
	}

	private static object ToHashSet(JsonElement json, Type innerType) =>
		typeof(Enumerable).GetMethod(nameof(Enumerable.ToHashSet))!.MakeGenericMethod(innerType)
			.Invoke(null, [ToEnumerable(json, innerType)])!;

	private static object ToList(JsonElement json, Type innerType)
	{
		int size = json.GetArrayLength();
		object?[] argumentsArray = [size];
		Type listType = typeof(List<>).MakeGenericType(innerType);
		object list = listType.GetConstructor([typeof(int)])!.Invoke(argumentsArray);
		MethodInfo addMethod = listType.GetMethod(nameof(List<object>.Add))!;
		var deserialize = DeserializerForType(innerType);
		foreach (JsonElement element in json.EnumerateArray())
		{
			argumentsArray[0] = deserialize(element);
			addMethod.Invoke(list, argumentsArray);
		}

		return list;
	}

	private static object ToImmutableArray(JsonElement json, Type innerType)
	{
		int size = json.GetArrayLength();
		object?[] argumentsArray = [size];
		object builder = typeof(ImmutableArray).GetMethod(nameof(ImmutableArray.CreateBuilder))!
			.MakeGenericMethod(innerType).Invoke(null, argumentsArray)!;
		MethodInfo addMethod = builder.GetType().GetMethod(nameof(ImmutableArray<object>.Builder.Add))!;
		var deserialize = DeserializerForType(innerType);
		foreach (JsonElement element in json.EnumerateArray())
		{
			argumentsArray[0] = deserialize(element);
			addMethod.Invoke(builder, argumentsArray);
		}

		return builder.GetType().GetMethod(nameof(ImmutableArray<object>.Builder.DrainToImmutable))!
			.Invoke(builder, null)!;
	}

	private static object ToImmutableSet(JsonElement json, Type innerType) =>
		typeof(ImmutableHashSet).GetMethod(nameof(ImmutableHashSet.ToImmutableHashSet))!.MakeGenericMethod(innerType)
			.Invoke(null, [ToEnumerable(json, innerType)])!;
}
