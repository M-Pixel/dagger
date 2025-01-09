using Dagger.Introspection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Dagger.Functions;
using static Dagger.SyntaxTree;

namespace Dagger;

static class Structures
{
	public static IEnumerable<MemberDeclarationSyntax> Generate(Schema schema)
		=> schema.Types.Select<Introspection.Type, MemberDeclarationSyntax?>
		(
			type =>
			{
				if (IsCustomScalar(type))
					return GenerateCustomScalar(type);
				if (IsEnum(type))
					return GenerateEnum(type);
				if (!type.InputFields.IsDefaultOrEmpty)
					return GenerateInputRecord(type);
				return null;
			}
		)
			.Where(syntax => syntax != null)!;

	static RecordDeclarationSyntax GenerateCustomScalar(Introspection.Type type)
	{
		string formattedName = FormatName(type.Name);
		return RecordDeclaration(formattedName)
			.AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword)
			.AddDocumentationComments(type)
			.AddParameterListParameters(Parameter(Identifier("Value")).WithType(IdentifierName("String")))
			.WithBaseListTypes([GenericName("ISelfDeserializable", formattedName)])
			.WithBody()
			.AddMembers
			(
				MethodDeclaration(GenericName("ValueTask", "String"), "_Serialize")
					.WithExplicitInterfaceSpecifier("ISelfSerializable")
					.WithExpressionBody
					(
						InvocationExpression
						(
							MemberAccessExpression("ValueTask", "FromResult"),
							IdentifierName("Value")
						)
					),
				MethodDeclaration(IdentifierName(formattedName), "_Deserialize")
					.AddModifiers(SyntaxKind.StaticKeyword)
					.WithExplicitInterfaceSpecifier(GenericName("ISelfDeserializable", formattedName))
					.AddParameterListParameters(Parameter(IdentifierName("String"), "asString"))
					.WithExpressionBody(ImplicitObjectCreationExpression(IdentifierName("asString")))
			);
	}

	static EnumDeclarationSyntax GenerateEnum(Introspection.Type type)
		=> EnumDeclaration(type.Name)
			.AddModifiers(SyntaxKind.PublicKeyword)
			.AddAttribute("JsonConverter", TypeOfExpression(GenericName("JsonStringEnumConverter", type.Name)))
			.AddDocumentationComments(type)
			.WithMembers
			(
				type.EnumValues
					.OrderBy(enumValue => enumValue.Name, StringComparer.Ordinal)
					.Select(enumValue => EnumMemberDeclaration(enumValue.Name))
			);

	static RecordDeclarationSyntax GenerateInputRecord(Introspection.Type type)
		=> RecordDeclaration(FormatName(type.Name))
			.AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword)
			.AddDocumentationComments
			(
				type.InputFields
					.Select(field => XmlParamElement(FormatName(field.Name), XmlParagraphs(field.Description)))
			)
			.WithParameters
			(
				type.InputFields
					.OrderBy(inputField => inputField.Name, StringComparer.Ordinal)
					.Select
					(
						inputField => Parameter(FormatType(inputField.Type, isInput: true), FormatName(inputField.Name))
							.WithDefault
							(
								inputField.Type.IsOptional() ? EqualsValueClause(NullLiteralExpression) : null
							)
					)
			)
			.WithBody()
			.AddMembers
			(
				MethodDeclaration(IdentifierName("OperationArgument"), "AsOperationArguments")
					.AddModifiers(SyntaxKind.InternalKeyword)
					.WithBody
					(
						OperationArgumentConversionStatements(type.InputFields, FormatName, isForRootQueryObject: false)
							.Append(ReturnStatement(IdentifierName("_arguments_")))
					)
			);
}
