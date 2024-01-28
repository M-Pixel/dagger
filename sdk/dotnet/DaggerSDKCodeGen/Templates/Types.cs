using DaggerSDK.Introspection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static DaggerSDK.Functions;
using static DaggerSDK.SyntaxTree;

namespace DaggerSDK;

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
				if (type.InputFields.Length > 0)
					return GenerateInputRecord(type);
				return null;
			}
		)
			.Where(syntax => syntax != null)!;

	static RecordDeclarationSyntax GenerateCustomScalar(Introspection.Type type)
		=> RecordDeclaration(FormatName(type.Name))
			.AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword)
			.AddParameterListParameters(Parameter(Identifier("Value")).WithType(IdentifierName("string")))
			.AddDocumentationComments(type);

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
					)
			)
			.WithBody()
			.AddMembers
			(
				MethodDeclaration(ArrayType("OperationArgument"), "AsOperationArguments")
					.AddModifiers(SyntaxKind.InternalKeyword)
					.WithExpressionBody
					(
						CollectionExpression
						(
							OperationArgumentConversionExpressions(type.InputFields, FormatName, isForRootClient: false)
						)
					)
			);
}
