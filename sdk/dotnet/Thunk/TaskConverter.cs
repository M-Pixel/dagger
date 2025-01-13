using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dagger.Thunk;

class TaskConverter<T> : JsonConverter<Task<T>>
{
	public override bool CanConvert(Type typeToConvert)
		=> typeToConvert == typeof(Task<T>) || typeToConvert.BaseType == typeof(Task<T>);

	public override Task<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> Task.FromResult(((JsonConverter<T>)options.GetConverter(typeof(T))).Read(ref reader, typeof(T), options)!);

	public override void Write(Utf8JsonWriter writer, Task<T> value, JsonSerializerOptions options)
		=> ((JsonConverter<T>)options.GetConverter(typeof(T))).Write(writer, value.Result, options);
}

class ValueTaskConverter<T> : JsonConverter<ValueTask<T>>
{
	public override ValueTask<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> ValueTask.FromResult
		(
			((JsonConverter<T>)options.GetConverter(typeof(T))).Read(ref reader, typeof(T), options)!
		);

	public override void Write(Utf8JsonWriter writer, ValueTask<T> value, JsonSerializerOptions options)
		=> ((JsonConverter<T>)options.GetConverter(typeof(T))).Write(writer, value.Result, options);
}

class TaskConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		return (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Task<>)) ||
			(
				typeToConvert.BaseType is { IsGenericType: true } &&
				typeToConvert.BaseType.GetGenericTypeDefinition() == typeof(Task<>)
			);
	}

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		Type wrappedType = typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Task<>)
			? typeToConvert.GetGenericArguments()[0]
			: typeToConvert.BaseType!.GetGenericArguments()[0];
		return (JsonConverter)Activator.CreateInstance(typeof(TaskConverter<>).MakeGenericType(wrappedType))!;
	}
}

class ValueTaskConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		if (!typeToConvert.IsGenericType)
			return false;

		return typeToConvert.GetGenericTypeDefinition() == typeof(ValueTask<>);
	}

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		Type wrappedType = typeToConvert.GetGenericArguments()[0];
		return (JsonConverter)Activator.CreateInstance(typeof(ValueTaskConverter<>).MakeGenericType(wrappedType))!;
	}
}
