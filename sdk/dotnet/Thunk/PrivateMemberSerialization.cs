using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Dagger.Thunk;

// This is NOT as robust as it should be.  E.g. it doesn't handle property ignore conditions.  But it's a holdover until
// I switch to pre-compiled serializers @ Prime instead of regular @ thunk.
static class PrivateMemberSerialization
{
	internal static Action<JsonTypeInfo> TypeInfoModifierFactory(Assembly moduleAssembly) => typeInfo =>
	{
		if (typeInfo.Kind != JsonTypeInfoKind.Object || typeInfo.Type.Assembly != moduleAssembly)
			return;

		BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

		foreach (FieldInfo field in typeInfo.Type.GetFields(bindingFlags))
		{
			if
			(
				field.IsSpecialName || typeInfo.Properties.Any(property => property.Name == field.Name) ||
				field.GetCustomAttribute<JsonIgnoreAttribute>() != null
			)
				continue;

			JsonPropertyInfo propertyInfo = typeInfo.CreateJsonPropertyInfo(field.FieldType, field.Name);
			propertyInfo.Get = obj => field.GetValue(obj);
			propertyInfo.Set = (obj, value) => field.SetValue(obj, value);

			if (field.GetCustomAttribute<JsonConverterAttribute>() is { } converterAttr)
				propertyInfo.CustomConverter = converterAttr.ConverterType != null
					? (JsonConverter?)Activator.CreateInstance(converterAttr.ConverterType)
					: converterAttr.CreateConverter(field.FieldType);

			typeInfo.Properties.Add(propertyInfo);
		}

		foreach (PropertyInfo property in typeInfo.Type.GetProperties(bindingFlags))
		{
			// TODO: Ignore properties that aren't auto-properties, and make sure that introspection treats them as methods
			if
			(
				property.GetMethod == null || property.SetMethod == null ||
				typeInfo.Properties.Any(jsonProperty => jsonProperty.Name == property.Name) ||
				property.GetCustomAttribute<JsonIgnoreAttribute>() != null
			)
				continue;

			JsonPropertyInfo propertyInfo = typeInfo.CreateJsonPropertyInfo(property.PropertyType, property.Name);
			propertyInfo.Get = obj => property.GetValue(obj);
			propertyInfo.Set = (obj, value) => property.SetValue(obj, value);

			if (property.GetCustomAttribute<JsonConverterAttribute>() is { } converterAttr)
				propertyInfo.CustomConverter = converterAttr.ConverterType != null
					? (JsonConverter?)Activator.CreateInstance(converterAttr.ConverterType)
					: converterAttr.CreateConverter(property.PropertyType);

			typeInfo.Properties.Add(propertyInfo);
		}
	};
}
