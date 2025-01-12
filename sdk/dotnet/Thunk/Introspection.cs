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
using static Alias;

class Introspection
{
	public readonly string? DefaultPath;
	public readonly string[]? DefaultIgnores;
	public TypeDef StaticObject;
	public IImmutableDictionary<string, ElementDocumentation>? MembersDocumentation;
	private Module _module = Query.FromDefaultSession.GetModule();
	private readonly IEnumerable<Type> _exportedTypes;
	private readonly NullabilityInfoContext _nullabilityContext = new();
	private readonly TypeDef _staticObjectOriginal;
	private readonly string _moduleName;


	public Introspection(Assembly moduleAssembly, string moduleName)
	{
		_exportedTypes = moduleAssembly.ExportedTypes;
		StaticObject = Query.FromDefaultSession.GetTypeDef().WithObject(moduleName);
		_staticObjectOriginal = StaticObject;
		_moduleName = moduleName;
		ApplyDirectoryAttribute(moduleAssembly.GetCustomAttributesData(), ref DefaultPath, ref DefaultIgnores);
	}


	public Module Build(ElementDocumentation assemblyDocumentation)
	{
		bool sameNameClassExists = false;
		bool sameNameStaticSuffixClassExists = false;

		MembersDocumentation = assemblyDocumentation.Members;

		foreach (Type exportedType in _exportedTypes)
		{
			Console.WriteLine(exportedType.AssemblyQualifiedName);
			if (IsIneligible(exportedType))
				continue;

			if (exportedType.IsInterface)
			{
				_module = _module
					.WithInterface(new InterfaceIntrospection{ Module = this, Type = exportedType }.Build());
			}
			else if (exportedType.IsEnum)
			{
				// Can't set type to enum yet
			}
			else if (!exportedType.IsAbstract)
			{
				_module = _module
					.WithObject(new ObjectIntrospection{ Module = this, Type = exportedType }.Build());
			}
			else if (exportedType.IsSealed)
			{
				Console.WriteLine("\tIs static.");
				new StaticObjectIntrospection{ Module = this, Type = exportedType }.Build();
				continue;
			}

			if (exportedType.Name == _moduleName)
				sameNameClassExists = true;
			else if (exportedType.Name == _moduleName + "Static")
				sameNameStaticSuffixClassExists = true;
		}

		if (_staticObjectOriginal != StaticObject)
		{
			if (sameNameClassExists)
			{
				if (sameNameStaticSuffixClassExists)
					throw new Exception
					(
						$"Can't have all three of non-static types {_moduleName} and {_moduleName}Static, and static " +
						$"methods, in the same module."
					);

				StaticObject = StaticObject.WithObject($"{_moduleName}Static");
			}

			_module = _module.WithObject(StaticObject);
		}

		return _module;
	}

	private static bool IsIneligible(Type exportedType)
	{
		if (!exportedType.IsPublic)
		{
			Console.WriteLine("\tIneligible because not public.");
			return true;
		}
		if (exportedType.IsGenericType)
		{
			Console.WriteLine("\tIneligible because generic.");
			return true;
		}
		if (exportedType.IsAssignableTo(typeof(Attribute)))
		{
			Console.WriteLine("\tIneligible because attribute.");
			return true;
		}
		return false;
	}

	public TypeDef TypeReferenceWithNullability(FieldInfo field, bool input) =>
		TypeReferenceWithNullability(_nullabilityContext.Create(field), input);

	public TypeDef TypeReferenceWithNullability(ParameterInfo parameter, bool input) =>
		TypeReferenceWithNullability(_nullabilityContext.Create(parameter), input);

	public TypeDef TypeReferenceWithNullability(PropertyInfo property, bool input) =>
		TypeReferenceWithNullability(_nullabilityContext.Create(property), input);

	static TypeDef TypeReferenceWithNullability(NullabilityInfo nullability, bool input)
	{
		Type type = nullability.Type;
		TypeDef typeDefinition = TypeReference(type, input);
		if (!type.IsValueType && nullability.WriteState == NullabilityState.Nullable)
			typeDefinition = typeDefinition.WithOptional(true);
		return typeDefinition;
	}

	public static TypeDef TypeReference(Type type, bool input)
	{
		if (type.IsByRef)
			throw new NotSupportedException("ref/in/out parameters are fundamentally incompatible with Dagger.");

		var dag = Query.FromDefaultSession;
		if (type.Name == "String")
			return dag.GetTypeDef().WithKind(TypeDefKind.STRING_KIND);
		if (type.IsValueType)
		{
			if (type.FullName == typeof(ValueTask).FullName)
				return input
					? throw new NotSupportedException("Dagger dotnet binder doesn't support ValueTask parameters.")
					: dag.GetTypeDef().WithKind(TypeDefKind.VOID_KIND);
			if (type.IsGenericType)
			{
				Type genericType = type.GetGenericTypeDefinition();
				if (genericType.FullName == typeof(Nullable<>).FullName)
					return TypeReference(type.GenericTypeArguments[0], input).WithOptional(true);
				if (genericType.FullName == typeof(ValueTask<>).FullName)
					return input
						? throw new NotSupportedException("Dagger Dotnet binder doesn't support ValueTask<> parameters.")
						: TypeReference(type.GenericTypeArguments[0], false);

				throw new NotSupportedException
				(
					$"Dagger does not support introspection of generic structs (apart from Nullable and ValueTask).  " +
					$"Cannot expose {type.FullName}."
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
							$"Dotnet primitive type {type.FullName} cannot be marshalled through Dagger module calls.  " +
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
			Type genericType = type.GetGenericTypeDefinition();
			if (genericType.FullName == typeof(Task<>).FullName)
				return input
					? throw new NotSupportedException("Dagger Dotnet binder doesn't support Task<> parameters.")
					: TypeReference(type.GenericTypeArguments[0], false);
			if
			(
				genericType.IsArray || genericType.FullName is "System.Collections.Generic.HashSet`1" or
					"System.Collections.Generic.IAsyncEnumerable`1" or "System.Collections.Generic.ICollection`1" or
					"System.Collections.Generic.IEnumerable`1" or "System.Collections.Generic.IList`1" or
					"System.Collections.Generic.IReadOnlyCollection`1" or "System.Collections.Generic.IReadOnlyList`1"
					or "System.Collections.Generic.ISet`1" or "System.Collections.Generic.LinkedList`1" or
					"System.Collections.Generic.List`1" or "System.Collections.Generic.Queue`1" or
					"System.Collections.Generic.SortedList`1" or "System.Collections.Generic.SortedSet`1" or
					"System.Collections.Generic.Stack`1" or "System.Collections.Immutable.IImmutableList`1" or
					"System.Collections.Immutable.IImmutableQueue`1" or "System.Collections.Immutable.IImmutableSet`1"
					or "System.Collections.Immutable.IImmutableStack`1" or
					"System.Collections.Immutable.ImmutableArray`1" or "System.Collections.Immutable.ImmutableHashSet`1"
					or "System.Collections.Immutable.ImmutableQueue`1" or
					"System.Collections.Immutable.ImmutableSortedSet`1" or
					"System.Collections.Immutable.ImmutableStack`1" or "System.Collections.Concurrent.ConcurrentQueue`1"
					or "System.Collections.Concurrent.ConcurrentStack`1"
			)
				return dag.GetTypeDef().WithListOf(TypeReference(type.GenericTypeArguments[0], input));
			throw new NotSupportedException
			(
				$"Dagger does not support introspection of generic classes (apart from enumerables and task).  " +
				$"Cannot expose {type.FullName}."
			);
		}
		else if (type.FullName == typeof(Task).FullName)
			return input
				? throw new NotSupportedException("Dagger Dotnet binder doesn't support Task parameters.")
				: dag.GetTypeDef().WithKind(TypeDefKind.VOID_KIND);
		else if
		(
			type.FullName == typeof(JsonObject).FullName ||
			type.FullName == typeof(JsonElement.ObjectEnumerator).FullName
		)
			return dag.GetTypeDef().WithObject("JSON");
		else if
		(
			type.FullName == typeof(JsonArray).FullName ||
			type.FullName == typeof(JsonElement.ArrayEnumerator).FullName
		)
			return dag.GetTypeDef().WithListOf(dag.GetTypeDef().WithObject("JSON"));
		// TODO: If type isn't from own assembly, add it to object declarations

		return dag.GetTypeDef().WithObject(type.Name);
	}

	public static void ApplyDirectoryAttribute
	(
		IEnumerable<CustomAttributeData> customAttributes,
		ref string? defaultPath,
		ref string[]? ignorePatterns
	)
	{
		foreach (CustomAttributeData attribute in customAttributes)
		{
			if (attribute.AttributeType.Name != nameof(DirectoryFromContextAttribute))
				continue;

			foreach (CustomAttributeNamedArgument argument in attribute.NamedArguments)
				if
				(
					argument.MemberName == nameof(DirectoryFromContextAttribute.Inherit) &&
					argument.TypedValue.Value == (object?)false
				)
				{
					defaultPath = null;
					ignorePatterns = null;
				}
			foreach (CustomAttributeNamedArgument argument in attribute.NamedArguments)
			{
				if (argument.MemberName == nameof(DirectoryFromContextAttribute.DefaultPath))
					defaultPath = argument.TypedValue.Value as string;
				else if (argument.MemberName == nameof(DirectoryFromContextAttribute.Ignore))
					ignorePatterns = argument.TypedValue.Value as string[];
			}
			break;
		}
	}
}

abstract class ObjectlikeIntrospection<TTypeDefNullability>
{
	public required Introspection Module;
	public required Type Type;
	public ElementDocumentation Documentation;
	private string? _defaultPath;
	private string[]? _defaultIgnore;


	public TTypeDefNullability Build()
	{
		Module.MembersDocumentation?.TryGetValue(Type.FullName!, out Documentation);
		_defaultPath = Module.DefaultPath;
		_defaultIgnore = Module.DefaultIgnores;
		Introspection.ApplyDirectoryAttribute(Type.GetCustomAttributesData(), ref _defaultPath, ref _defaultIgnore);
		return BuildImplementation();
	}

	protected abstract TTypeDefNullability BuildImplementation();

	protected void AddMembers(ref TypeDef? typeDefinition)
	{
		var dag = Query.FromDefaultSession;
		TypeDef? bareTypeDefinition = typeDefinition;
		bool hasConstructor = false;

		BindingFlags bindingFlags = typeDefinition == null
			? BindingFlags.Public | BindingFlags.Static
			: BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
		if (typeDefinition != null)
			foreach (ConstructorInfo constructorInfo in Type.GetConstructors(bindingFlags))
			{
				Console.WriteLine("\tconstructor");
				ElementDocumentation memberDocumentation = new();
				Documentation.Members?.TryGetValue(constructorInfo.Name, out memberDocumentation);

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
				typeDefinition = typeDefinition.WithConstructor(constructor);
			}

		// TODO: Events

		foreach (FieldInfo fieldInfo in Type.GetFields(bindingFlags))
		{
			Console.WriteLine($"\tfield {fieldInfo.Name}");
			ElementDocumentation memberDocumentation = new();
			Documentation.Members?.TryGetValue(fieldInfo.Name, out memberDocumentation);

			TypeDef targetDefinition = fieldInfo.IsStatic ? Module.StaticObject : typeDefinition!;

			targetDefinition = targetDefinition.WithField
			(
				fieldInfo.Name,
				Module.TypeReferenceWithNullability(fieldInfo, false),
				memberDocumentation.Summary
			);

			if (fieldInfo is { IsInitOnly: false, IsStatic: false })
				try
				{
					Function setter = dag.Function($"With{fieldInfo.Name}", bareTypeDefinition!)
						.WithArg("value", Introspection.TypeReference(fieldInfo.FieldType, true));
					if (memberDocumentation.Summary != null)
						setter = setter.WithDescription(memberDocumentation.Summary);
					targetDefinition = targetDefinition.WithFunction(setter);
				}
				catch (Exception)
				{
					/* If I can't create setter, just omit it. */
				}

			if (fieldInfo.IsStatic)
				Module.StaticObject = targetDefinition;
			else
				typeDefinition = targetDefinition;
		}

		foreach (MethodInfo methodInfo in Type.GetMethods(bindingFlags))
		{
			Console.WriteLine($"\tmethod {methodInfo.Name}");

			if (methodInfo.IsSpecialName || methodInfo.Name.StartsWith('<'))
			{
				Console.WriteLine("\t\tIgnoring special name");
				continue;
			}
			if (methodInfo.Name is "GetType" or "Deconstruct" or "Equals")
			{
				Console.WriteLine("\t\tIgnoring prohibited name");
				continue;
			}

			ElementDocumentation memberDocumentation = new();
			Documentation.Members?.TryGetValue(methodInfo.Name, out memberDocumentation);

			Function function = dag.Function
			(
				methodInfo.Name,
				Module.TypeReferenceWithNullability(methodInfo.ReturnParameter, false)
			);
			if (memberDocumentation.Summary != null)
				function = function.WithDescription(memberDocumentation.Summary);
			function = AddParameters(function, methodInfo.GetParameters(), memberDocumentation.Members);
			if (methodInfo.IsStatic)
				Module.StaticObject = Module.StaticObject.WithFunction(function);
			else
				typeDefinition = typeDefinition!.WithFunction(function);
		}

		foreach (PropertyInfo propertyInfo in Type.GetProperties(bindingFlags))
		{
			Console.WriteLine($"\tproperty {propertyInfo.Name}");
			ElementDocumentation memberDocumentation = new();
			Documentation.Members?.TryGetValue(propertyInfo.Name, out memberDocumentation);

			bool isRequired = propertyInfo.GetCustomAttributesData()
				.Any(attribute => attribute.AttributeType.Name == nameof(RequiredMemberAttribute));
			if (isRequired)
				throw new NotImplementedException(
					$"Dagger Dotnet runtime does not yet support required properties.  Initialize {propertyInfo.Name} through the constructor instead (or make it non-public).");
			MethodInfo? getMethod = propertyInfo.GetMethod;
			if (getMethod?.IsPublic ?? false)
			{
				TypeDef targetDefinition = getMethod.IsStatic ? Module.StaticObject : typeDefinition!;
				targetDefinition = targetDefinition.WithField
				(
					propertyInfo.Name,
					Module.TypeReferenceWithNullability(propertyInfo, false),
					memberDocumentation.Summary
				);
				if (getMethod.IsStatic)
					Module.StaticObject = targetDefinition;
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
							.WithArg("value", Introspection.TypeReference(propertyInfo.PropertyType, true));
						if (memberDocumentation.Summary != null)
							setter = setter.WithDescription(memberDocumentation.Summary);
						typeDefinition = typeDefinition!.WithFunction(setter);
					}
					catch (Exception)
					{
						/* If I can't create setter, just omit it. */
					}
				}
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

			string? defaultPath = null;
			string[]? ignorePatterns = null;
			string? parameterTypeName = parameterInfo.ParameterType.FullName;
			if
			(
				parameterTypeName != null &&
				parameterTypeName.StartsWith("Dagger.Generated.") && parameterTypeName.EndsWith(".Directory")
			)
			{
				defaultPath = _defaultPath;
				ignorePatterns = _defaultIgnore;
				Introspection.ApplyDirectoryAttribute
				(
					parameterInfo.GetCustomAttributesData(),
					ref defaultPath,
					ref ignorePatterns
				);
			}

			function = function.WithArg
			(
				parameterInfo.Name,
				Module.TypeReferenceWithNullability(parameterInfo, true),
				documentation?.TryGetValue(parameterInfo.Name, out ElementDocumentation parameterDocumentation) ?? false
					? parameterDocumentation.Summary
					: null,
				parameterInfo is { HasDefaultValue: true, RawDefaultValue: not null }
					? new JSON(parameterInfo.RawDefaultValue.ToString()!)
					: null,
				defaultPath,
				ignorePatterns
			);
		}

		return function;
	}
}

class InterfaceIntrospection : ObjectlikeIntrospection<TypeDef>
{
	protected override TypeDef BuildImplementation()
	{
		TypeDef typeDefinition = DAG.GetTypeDef().WithInterface(Type.Name, Documentation.Summary);
		AddMembers(ref typeDefinition!);
		return typeDefinition;
	}
}

class ObjectIntrospection : ObjectlikeIntrospection<TypeDef>
{
	protected override TypeDef BuildImplementation()
	{
		TypeDef typeDefinition = DAG.GetTypeDef().WithObject(Type.Name, Documentation.Summary);
		AddMembers(ref typeDefinition!);
		return typeDefinition;
	}
}

class StaticObjectIntrospection : ObjectlikeIntrospection<TypeDef?>
{
	protected override TypeDef? BuildImplementation()
	{
		TypeDef? nullTypeDef = null;
		AddMembers(ref nullTypeDef);
		return nullTypeDef;
	}
}
