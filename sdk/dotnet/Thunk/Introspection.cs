using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Dagger.Generated.ModuleTest;
using Module = Dagger.Generated.ModuleTest.Module;

namespace Dagger.Thunk;

class Introspection
{
	private Module _module = Query.FromDefaultSession.GetModule();
	private readonly IEnumerable<Type> _exportedTypes;
	private readonly NullabilityInfoContext _nullabilityContext = new();
	private TypeDef _moduleStatic;
	private readonly TypeDef _moduleStaticOriginal;
	private readonly string _moduleName;


	public Introspection(Assembly moduleAssembly, string moduleName)
	{
		_exportedTypes = moduleAssembly.ExportedTypes;
		_moduleStatic = Query.FromDefaultSession.GetTypeDef().WithObject(moduleName);
		_moduleStaticOriginal = _moduleStatic;
		_moduleName = moduleName;
	}


	public Module BuildModule(ElementDocumentation assemblyDocumentation)
	{
		bool sameNameClassExists = false;

		foreach (Type exportedType in _exportedTypes)
		{
			Console.WriteLine(exportedType.AssemblyQualifiedName);
			if (IsIneligible(exportedType))
				continue;

			if (exportedType.IsInterface)
			{
				ElementDocumentation interfaceDocumentation = new();
				assemblyDocumentation.Members?.TryGetValue(exportedType.FullName!, out interfaceDocumentation);

				TypeDef typeDefinition = Query.FromDefaultSession.GetTypeDef()
					.WithInterface(exportedType.Name, interfaceDocumentation.Summary);

				AddMembers(ref typeDefinition!, exportedType, assemblyDocumentation.Members);
				_module = _module.WithInterface(typeDefinition);
			}
			else if (exportedType.IsEnum)
			{
				// Can't set type to enum yet
			}
			else if (!exportedType.IsAbstract)
			{
				ElementDocumentation objectDocumentation = new();
				assemblyDocumentation.Members?.TryGetValue(exportedType.FullName!, out objectDocumentation);

				TypeDef typeDefinition = Query.FromDefaultSession.GetTypeDef()
					.WithObject(exportedType.Name, objectDocumentation.Summary);

				AddMembers(ref typeDefinition!, exportedType, assemblyDocumentation.Members);
				_module = _module.WithObject(typeDefinition);
			}
			else if (exportedType.IsSealed)
			{
				TypeDef? nullTypeDef = null;
				AddMembers(ref nullTypeDef, exportedType, assemblyDocumentation.Members);
				continue;
			}

			if (exportedType.Name == _moduleName)
				sameNameClassExists = true;
		}

		if (_moduleStaticOriginal != _moduleStatic)
		{
			if (sameNameClassExists)
				_moduleStatic = _moduleStatic.WithObject($"{_moduleName}Statics");
			_module = _module.WithObject(_moduleStatic);
		}

		return _module;
	}

	private static bool IsIneligible(Type exportedType)
	{
		return !exportedType.IsPublic || exportedType.FullName == null || exportedType.IsGenericType ||
		       exportedType.IsAssignableTo(typeof(Attribute));
	}

	void AddMembers
	(
		ref TypeDef? typeDefinition,
		Type type,
		IImmutableDictionary<string, ElementDocumentation>? objectDocumentation
	)
	{
		var dag = Query.FromDefaultSession;
		TypeDef? bareTypeDefinition = typeDefinition;
		bool hasConstructor = false;

		BindingFlags bindingFlags = typeDefinition == null
			? BindingFlags.Public
			: BindingFlags.Public | BindingFlags.Static;
		foreach (var memberInfo in type.GetMembers(bindingFlags))
		{
			Console.WriteLine($"\t{memberInfo.Name}");
			if (memberInfo.MemberType is MemberTypes.TypeInfo or MemberTypes.Custom or MemberTypes.NestedType)
				continue;
			if (memberInfo.Name is "GetType" or ".ctor")
				continue;

			ElementDocumentation memberDocumentation = new();
			objectDocumentation?.TryGetValue(memberInfo.Name, out memberDocumentation);
			switch (memberInfo)
			{
				case ConstructorInfo constructorInfo:
					ParameterInfo[] parameters = constructorInfo.GetParameters();
					if (parameters.Length == 0)
						continue;
					if (hasConstructor)
						throw new Exception("Dagger does not support overloaded constructors.");
					hasConstructor = true;
					// bareTypeDefinition and typeDefinition assumed to be non-null because constructorInfo won't be
					// encountered with BindingFlags.Static.
					Function constructor = dag.Function("", bareTypeDefinition!);
					if (memberDocumentation.Summary != null)
						constructor = constructor.WithDescription(memberDocumentation.Summary);
					constructor = AddParameters(constructor, parameters, memberDocumentation.Members);
					typeDefinition = typeDefinition!.WithConstructor(constructor);
					break;

				case EventInfo:
					// TODO
					break;

				case FieldInfo fieldInfo:
				{
					TypeDef targetDefinition = fieldInfo.IsStatic ? _moduleStatic : typeDefinition!;

					targetDefinition = targetDefinition.WithField
					(
						fieldInfo.Name,
						TypeReferenceWithNullability(_nullabilityContext.Create(fieldInfo), false),
						memberDocumentation.Summary
					);

					if (fieldInfo is { IsInitOnly: false, IsStatic: false })
						try
						{
							Function setter = dag.Function($"With{fieldInfo.Name}", bareTypeDefinition!)
								.WithArg("value", TypeReference(fieldInfo.FieldType, true));
							if (memberDocumentation.Summary != null)
								setter = setter.WithDescription(memberDocumentation.Summary);
							targetDefinition = targetDefinition.WithFunction(setter);
						} catch (Exception) { /* If I can't create setter, just omit it. */ }

					if (fieldInfo.IsStatic)
						_moduleStatic = targetDefinition;
					else
						typeDefinition = targetDefinition;
					break;
				}

				case MethodInfo methodInfo:
					Function function = dag.Function
					(
						memberInfo.Name,
						TypeReferenceWithNullability(_nullabilityContext.Create(methodInfo.ReturnParameter), false)
					);
					function = AddParameters(function, methodInfo.GetParameters(), memberDocumentation.Members);
					if (methodInfo.IsStatic)
						_moduleStatic = _moduleStatic.WithFunction(function);
					else
						typeDefinition = typeDefinition!.WithFunction(function);
					break;

				case PropertyInfo propertyInfo:
					if (propertyInfo.GetCustomAttribute<RequiredMemberAttribute>() != null)
						throw new NotImplementedException($"Dagger Dotnet runtime does not yet support required properties.  Initialize {propertyInfo.Name} through the constructor instead (or make it internal).");
					MethodInfo? getMethod = propertyInfo.GetMethod;
					if (getMethod?.IsPublic ?? false)
					{
						TypeDef targetDefinition = getMethod.IsStatic ? _moduleStatic : typeDefinition!;
						targetDefinition = targetDefinition.WithField
						(
							propertyInfo.Name,
							TypeReferenceWithNullability(_nullabilityContext.Create(propertyInfo), false),
							memberDocumentation.Summary
						);
						if (getMethod.IsStatic)
							_moduleStatic = targetDefinition;
						else
							typeDefinition = targetDefinition;
					}

					MethodInfo? setMethod = propertyInfo.SetMethod;
					if (setMethod is { IsPublic: true, IsStatic: false })
					{
						Type[] setMethodReturnParameterModifiers =
							setMethod.ReturnParameter.GetRequiredCustomModifiers();
						if (!setMethodReturnParameterModifiers.Contains(typeof(IsExternalInit)))
						{
							try
							{
								Function setter = dag.Function($"With{propertyInfo.Name}", bareTypeDefinition!)
									.WithArg("value", TypeReference(propertyInfo.PropertyType, true));
								if (memberDocumentation.Summary != null)
									setter = setter.WithDescription(memberDocumentation.Summary);
								typeDefinition = typeDefinition!.WithFunction(setter);
							} catch (Exception) { /* If I can't create setter, just omit it. */ }
						}
					}
					break;
			}
		}
	}

	Function AddParameters
	(
		Function function,
		ParameterInfo[] parameters,
		IImmutableDictionary<string, ElementDocumentation>? documentation
	)
	{
		foreach (ParameterInfo parameterInfo in parameters)
		{
			if (parameterInfo.Name == null)
				continue;

			function = function.WithArg
			(
				parameterInfo.Name,
				TypeReferenceWithNullability(_nullabilityContext.Create(parameterInfo), true),
				documentation?.TryGetValue(parameterInfo.Name, out ElementDocumentation parameterDocumentation) ?? false
					? parameterDocumentation.Summary
					: null,
				parameterInfo is { HasDefaultValue: true, RawDefaultValue: not null }
					? new JSON(parameterInfo.RawDefaultValue.ToString()!)
					: null
			);
		}

		return function;
	}

	TypeDef TypeReferenceWithNullability(NullabilityInfo nullability, bool input)
	{
		Type type = nullability.Type;
		TypeDef typeDefinition = TypeReference(type, input);
		if (!type.IsValueType && nullability.WriteState == NullabilityState.Nullable)
			typeDefinition = typeDefinition.WithOptional(true);
		return typeDefinition;
	}

	TypeDef TypeReference(Type type, bool input)
	{
		if (type.IsByRef)
			throw new NotSupportedException("ref/in/out parameters are fundamentally incompatible with Dagger.");

		var dag = Query.FromDefaultSession;
		if (type.Name == "String")
			return dag.GetTypeDef().WithKind(TypeDefKind.STRING_KIND);
		if (type.IsValueType)
		{
			if (type == typeof(ValueTask))
				return input
					? throw new NotSupportedException("Dagger dotnet binder doesn't support ValueTask parameters.")
					: dag.GetTypeDef().WithKind(TypeDefKind.VOID_KIND);
			if (type.IsGenericType)
			{
				Type genericType = type.GetGenericTypeDefinition();
				if (genericType == typeof(Nullable<>))
					return TypeReference(type.GenericTypeArguments[0], input).WithOptional(true);
				if (genericType == typeof(ValueTask<>))
					return input
						? throw new NotSupportedException("Dagger Dotnet binder doesn't support ValueTask<> parameters.")
						: TypeReference(type.GenericTypeArguments[0], false);

				throw new NotSupportedException
				(
					$"Dagger does not support introspection of generic types.  Cannot expose {type.Name}."
				);
			}

			if (type.IsPrimitive)
			{
				switch (type.Name)
				{
					case "Boolean":
						return dag.GetTypeDef().WithKind(TypeDefKind.BOOLEAN_KIND);
					case "Int32":
						return dag.GetTypeDef().WithKind(TypeDefKind.INTEGER_KIND);
					case "Float":
						throw new NotImplementedException("Dagger currently lacks full float support");
					default:
						throw new NotSupportedException
						(
							$"Dotnet primitive type {type.Name} cannot be marshalled through Dagger module calls.  " +
							$"Only bool, int, and string are supported."
						);
				}
			}

			// It's a struct
		}
		else if (type.IsArray)
			return dag.GetTypeDef().WithListOf(TypeReference(type.GetElementType()!, input));
		else if (type.IsGenericType)
		{
			if (Deserializer.ListStrategies.ContainsKey(type.GetGenericTypeDefinition()))
				return dag.GetTypeDef().WithListOf(TypeReference(type.GenericTypeArguments[0], input));
			if (type.GetGenericTypeDefinition() == typeof(Task<>))
				return input
					? throw new NotSupportedException("Dagger Dotnet binder doesn't support Task<> parameters.")
					: TypeReference(type.GenericTypeArguments[0], false);
			throw new NotSupportedException
			(
				$"Dagger does not support introspection of generic types.  Cannot expose {type.Name}."
			);
		}
		else if (type == typeof(Task))
			return input
				? throw new NotSupportedException("Dagger Dotnet binder doesn't support Task parameters.")
				: dag.GetTypeDef().WithKind(TypeDefKind.VOID_KIND);
		else if (type == typeof(JsonObject) || type == typeof(JsonElement.ObjectEnumerator))
			return dag.GetTypeDef().WithObject("JSON");
		else if (type == typeof(JsonArray) || type == typeof(JsonElement.ArrayEnumerator))
			return dag.GetTypeDef().WithListOf(dag.GetTypeDef().WithObject("JSON"));

		// Else it's a class
		return dag.GetTypeDef().WithObject(type.Name);
	}
}
