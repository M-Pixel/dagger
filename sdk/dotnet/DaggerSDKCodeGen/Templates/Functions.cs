using DaggerSDK.Introspection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static DaggerSDK.SyntaxTree;

namespace DaggerSDK;

static class Functions
{
	public static bool IsListOfObject(TypeReference type)
		=> type.OfType!.OfType!.IsObject();

	public static TypeReference GetArrayType(Field field)
	{
		TypeReference fieldType = field.Type;
		if (fieldType.IsOptional() == false)
			fieldType = fieldType.OfType!;
		if (fieldType.IsList() == false)
			throw new Exception($"Field {field.Name} is not a list");
		fieldType = fieldType.OfType!;
		if (fieldType.IsOptional() == false)
			fieldType = fieldType.OfType!;

		return fieldType;
	}

	public static List<Field> GetArrayField(Schema schema, Field field)
	{
		TypeReference fieldType = GetArrayType(field);
		Introspection.Type? schemaType = schema.Types.Get(fieldType.Name!);
		if (schemaType == null)
			throw new Exception($"Schema type {fieldType.Name} is null");

		List<Field> fields = new();
		Field? idField = null;
		// Only include scalar fields for now
		// TODO: include subtype too
		foreach (Field typeField in schemaType.Fields)
		{
			if (typeField.Type.IsScalar())
				fields.Add(typeField);
			// TODO: hack to fix requesting all fields from list of id-able objects, need better solution
			if (typeField.Name == "id")
			{
				idField = typeField;
				break;
			}
		}
		return idField != null
			? new List<Field>{ idField }
			: fields;
	}

	/// <returns><c>true</c> if the field returns an ID that should be converted into an object.</returns>
	public static bool ConvertsID(Field field)
	{
		if (field.Name == "id")
			return false;
		TypeReference type = field.Type;
		if (type.Kind == Introspection.TypeKind.NON_NULL)
			type = type.OfType!;
		if (type.Kind != Introspection.TypeKind.SCALAR)
			return false;
		// We only concern ourselves with the ID of the parent class, since this is really only meant for ID and Sync,
		// the only cases where we intentionally return an ID (leaf node) instead of an object.
		return type.Name == field.ParentObject!.Name + "ID";
	}

	/// <summary>Loops through the type reference to transform it into its SDK language.</summary>
	public static TypeSyntax FormatType(TypeReference typeReference, bool isInput, bool forceNonNull = false)
	{
		if (typeReference.IsOptional())
		{
			if (forceNonNull == false && (typeReference.IsList() == false || isInput))
				return NullableType
				(
					FormatType
					(
						new TypeReference(Introspection.TypeKind.NON_NULL, null, typeReference),
						isInput
					)
				);
		}
		else
			typeReference = typeReference.OfType!;

		if (isInput && typeReference.Name != null && typeReference.Name.EndsWith("ID"))
			typeReference = typeReference with { Name = string.Intern(typeReference.Name[..^2]) };

		return typeReference.Kind switch
		{
			Introspection.TypeKind.LIST => (isInput ? GenericName("IReadOnlyList") : GenericName("ImmutableArray"))
				.WithTypeArgumentList
				(
					TypeArgumentList(SingletonSeparatedList(FormatType(typeReference.OfType!, isInput)))
				),
			Introspection.TypeKind.SCALAR => typeReference.Name switch
			{
				nameof(Scalar.String) => PredefinedType(Token(SyntaxKind.StringKeyword)),
				nameof(Scalar.Int) => PredefinedType(Token(SyntaxKind.IntKeyword)),
				nameof(Scalar.Float) => PredefinedType(Token(SyntaxKind.FloatKeyword)),
				nameof(Scalar.Boolean) => PredefinedType(Token(SyntaxKind.BoolKeyword)),
				_ => IdentifierName(FormatName(typeReference.Name!))
			},
			Introspection.TypeKind.OBJECT or Introspection.TypeKind.INPUT_OBJECT or Introspection.TypeKind.ENUM =>
				IdentifierName(FormatName(typeReference.Name!)),
			_ => throw new Exception("Unexpected type kind " + typeReference.Kind)
		};
	}

	/// <summary>Change a type name into PascalCase</summary>
	public static string PascalCase(string name)
		=> string.Intern(char.ToUpper(name[0]) + name[1..]);

	/// <summary>Checks if a field is solvable (e.g. has a getter).</summary>
	public static bool RequiresSolving(Field field) => field.Type.IsScalar() || field.Type.IsList();

	/// <summary>Checks if the type is actually custom</summary>
	public static bool IsCustomScalar(Introspection.Type type)
	{
		switch (type.Name)
		{
			case nameof(Scalar.String):
			case nameof(Scalar.Int):
			case nameof(Scalar.Float):
			case nameof(Scalar.Boolean):
				return false;
			default:
				return type.Kind == Introspection.TypeKind.SCALAR;
		}
	}

	/// <summary>Checks if the type is actually custom</summary>
	public static bool IsCustomScalar(TypeReference type)
	{
		switch (type.ResolveName())
		{
			case nameof(Scalar.String):
			case nameof(Scalar.Int):
			case nameof(Scalar.Float):
			case nameof(Scalar.Boolean):
				return false;
			default:
				return type.ResolveKind() == Introspection.TypeKind.SCALAR;
		}
	}

	public static bool IsInputObject(TypeReference type)
	{
		while (type.Kind == Introspection.TypeKind.LIST || type.Kind == Introspection.TypeKind.NON_NULL)
			type = type.OfType!;
		return type.Kind == Introspection.TypeKind.INPUT_OBJECT;
	}

	public static bool IsEnum(Introspection.Type type)
		=> type.Kind == Introspection.TypeKind.ENUM
			// We ignore the internal GraphQL enums
			&& type.Name.StartsWith("__") == false;

	public static bool IsKeyword(string name) =>
		Enum.TryParse<SyntaxKind>(name + "Keyword", ignoreCase: true, out _);

	/// <summary>
	/// Formats a GraphQL name (e.g. object, field, arg) into a C# equivalent, avoiding collisions with reserved words.
	/// </summary>
	public static string FormatName(string name)
		=> name == "Query" ? "Client" : PascalCase(name);

	public static string FormatParameterName(string name)
		=> IsKeyword(name)
			? string.Intern('@' + name)
			: name;

	public static string FormatMethodName(Field field)
	{
		string methodName = FormatName(field.Name);
		string resolvedName = field.Type.ResolveName();
		if (field.Arguments.Length == 0)
		{
			if (resolvedName == nameof(Scalar.Boolean))
				return "Is" + methodName;
			if (methodName == FormatName(resolvedName) || methodName + "Id" == FormatName(resolvedName))
				return "Get" + methodName;
		}
		if (methodName == field.ParentObject!.Name)
			return "Sub" + methodName;
		return methodName;
	}

	public static IEnumerable<ExpressionSyntax> OperationArgumentConversionExpressions
	(
		IEnumerable<InputValue> inputs,
		Func<string, string> nameFormatter,
		bool isForRootClient
	)
		=> inputs.Select
		(
			argument =>
			{
				bool referenceTakesRawID =
					argument.Name == "id" && isForRootClient
					|| argument.Type.ResolveName().EndsWith("ID") == false;

				ExpressionSyntax valueExpression = IdentifierName(nameFormatter(argument.Name));

				if (IsCustomScalar(argument.Type) && referenceTakesRawID)
					valueExpression = argument.Type.IsOptional()
						? ConditionalAccessExpression(valueExpression, "Value")
						: MemberAccessExpression(valueExpression, "Value");
				else if (IsInputObject(argument.Type))
				{
					if (argument.Type.IsList())
					{
						var selectLambdaExpression = SimpleLambdaExpression
						(
							"element",
							InvocationExpression(MemberAccessExpression("element", "AsOperationArguments"))
						);
						valueExpression = argument.Type.IsOptional()
							? ConditionalAccessExpression
							(
								valueExpression,
								InvocationExpression(MemberBindingExpression(IdentifierName("Select")))
									.AddArgumentListArguments(selectLambdaExpression)
									.ChainInvocation("ToList")
							)
							: InvocationExpression(MemberAccessExpression(valueExpression, "Select"))
								.AddArgumentListArguments(selectLambdaExpression)
								.ChainInvocation("ToList");
					}
					else
						valueExpression = argument.Type.IsOptional()
							? ConditionalInvocationExpression(valueExpression, "AsOperationArguments")
							: InvocationExpression(MemberAccessExpression(valueExpression, "AsOperationArguments"));
				}

				return ObjectCreationExpression("OperationArgument")
					.AddArgumentListArguments
					(
						LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(argument.Name)),
						valueExpression,
						ParameterSerializationEnum(argument.Type, referenceTakesRawID),
						LiteralExpression(argument.Type.Kind == Introspection.TypeKind.LIST)
					);
			}
		);

	public static ExpressionSyntax ParameterSerializationEnum(TypeReference typeReference, bool takesRawId = false)
		=> typeReference.Kind switch
		{
			Introspection.TypeKind.SCALAR => typeReference.Name switch
			{
				// No quotes (numbers are technically enum - there are a finite # of choices)
				nameof(Scalar.Int) or nameof(Scalar.Float) or nameof(Scalar.Boolean)
					=> ParameterSerializationEnum("Enum"),
				nameof(Scalar.String) => ParameterSerializationEnum("String"),
				_ => ParameterSerializationEnum(takesRawId ? "String" : "Reference")
			},
			Introspection.TypeKind.ENUM => ParameterSerializationEnum("Enum"),
			Introspection.TypeKind.INPUT_OBJECT => ParameterSerializationEnum("Object"),
			Introspection.TypeKind.LIST or Introspection.TypeKind.NON_NULL
				=> ParameterSerializationEnum(typeReference.OfType!, takesRawId),
			_ => throw new NotImplementedException()
		};

	static ExpressionSyntax ParameterSerializationEnum(string name)
		=> MemberAccessExpression("ParameterSerialization", name);

	public static TSyntax AddDocumentationComments<TSyntax>(this TSyntax syntax, IDocumented symbol)
		where TSyntax : SyntaxNode
		=> symbol.Description == null
			? syntax
			: syntax.AddDocumentationComment(XmlSummaryElement(XmlParagraphs(symbol.Description)));

	public static TSyntax AddDocumentationComments<TSyntax>(this TSyntax syntax, Field field)
		where TSyntax : SyntaxNode
	{
		IEnumerable<XmlElementSyntax> comments = field.Arguments
			.Select(argument => XmlParamElement(FormatName(argument.Name), XmlParagraphs(argument.Description)));
		if (field.Description != null)
			comments = comments.Prepend(XmlSummaryElement(XmlParagraphs(field.Description)));
		return syntax.AddDocumentationComments(comments);
	}
}
