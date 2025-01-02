using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dagger;

class ImmutableArrayConverter<T> : JsonConverter<ImmutableArray<T>>
{
	private readonly object _Mutex = new();
	private JsonSerializerOptions? _nonrecursiveOptions;

	public override ImmutableArray<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> reader.TokenType is JsonTokenType.None or JsonTokenType.Null
			? ImmutableArray<T>.Empty
			: JsonSerializer.Deserialize<IEnumerable<T>>(ref reader, options)!.ToImmutableArray();

	public override void Write(Utf8JsonWriter writer, ImmutableArray<T> value, JsonSerializerOptions options)
		=> ((JsonConverter<ImmutableArray<T>>) UnRecurseOptions(options).GetConverter(typeof(ImmutableArray<T>)))
			.Write(writer, value, options);

	public override bool HandleNull => true;


	JsonSerializerOptions UnRecurseOptions(JsonSerializerOptions baseOptions)
	{
		lock (_Mutex)
		{
			if (_nonrecursiveOptions == null)
			{
				_nonrecursiveOptions = new JsonSerializerOptions(baseOptions);
				foreach (JsonConverter baseOptionsConverter in baseOptions.Converters)
					if (baseOptionsConverter is ImmutableArrayConverterFactory)
						_nonrecursiveOptions.Converters.Remove(baseOptionsConverter);
			}

			return _nonrecursiveOptions;
		}
	}
}

/// <summary>Allows undefined to be deserialized as empty ImmutableArray, instead of null or error.</summary>
class ImmutableArrayConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		if (!typeToConvert.IsGenericType)
			return false;

		return typeToConvert.GetGenericTypeDefinition() == typeof(ImmutableArray<>);
	}

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		Type wrappedType = typeToConvert.GetGenericArguments()[0];
		return (JsonConverter)Activator.CreateInstance(typeof(ImmutableArrayConverter<>).MakeGenericType(wrappedType))!;
	}
}
