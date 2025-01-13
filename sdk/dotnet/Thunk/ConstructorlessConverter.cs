using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Dagger.Thunk;

static class ConstructorlessConverter
{
	internal static void TypeInfoModifier(JsonTypeInfo typeInfo)
	{
		if (!ConstructorlessConverterFactory.InjectedConverters.Contains(typeInfo.Converter))
			return;
		typeInfo.CreateObject = () => RuntimeHelpers.GetUninitializedObject(typeInfo.Type);
	}
}

class SimpleClass
{
#pragma warning disable CS0414 // Field is assigned but its value is never used
	private string _simpleProperty = "";
#pragma warning restore CS0414 // Field is assigned but its value is never used
}

class ConstructorlessConverterFactory : JsonConverterFactory
{
	internal static ConcurrentBag<JsonConverter> InjectedConverters = new();

	private static readonly Type _defaultConverterType =
		new JsonSerializerOptions().GetConverter(typeof(SimpleClass)).GetType().GetGenericTypeDefinition();

	private readonly Assembly _moduleAssembly;

	public ConstructorlessConverterFactory(Assembly moduleAssembly)
	{
		_moduleAssembly = moduleAssembly;
	}

	public override bool CanConvert(Type typeToConvert)
	{
		// TODO: Skip types that have a [JsonConstructor]
		// TODO: Also consider Dagger-exported types from dependency assemblies
		const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		// Is class and has no parameterless constructor.
		return !typeToConvert.IsValueType && typeToConvert.Assembly == _moduleAssembly &&
			typeToConvert.GetConstructor(bindingFlags, []) == null;
	}

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var converter = (JsonConverter)Activator.CreateInstance(_defaultConverterType.MakeGenericType(typeToConvert))!;
		InjectedConverters.Add(converter);
		return converter;
	}
}
