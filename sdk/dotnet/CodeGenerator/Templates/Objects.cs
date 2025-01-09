using Dagger.Introspection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Dagger.Functions;
using static Dagger.SyntaxTree;

namespace Dagger;

static class Classes
{
	public static IEnumerable<MemberDeclarationSyntax> Generate(Schema schema)
		=> schema.Types
			.Where(type => type.Fields.Length > 0 && type.Name.StartsWith('_') == false)
			.Select(type => GenerateObject(schema, type));

	static MemberDeclarationSyntax GenerateObject(Schema schema, Introspection.Type type)
	{
		string formattedName = FormatName(type.Name);
		ClassDeclarationSyntax result = ClassDeclaration(formattedName)
			.AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword)
			.AddDocumentationComments(type)
			.WithBaseListTypes(["ObjectClient"])
			.WithMembers
			(
				type.Fields
					.Where(field => field.Type.IsScalar())
					.Select
					(
						field => PropertyDeclaration
						(
							Nullable(FormatType(field.Type, isInput: false)),
							"Cached" + FormatName(field.Name)
						)
							.AddModifiers(SyntaxKind.InternalKeyword)
							.AddAccessorListAccessors
							(
								AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
									.AddModifiers(SyntaxKind.PrivateKeyword)
									.WithSemicolonToken(),
								AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
									.WithSemicolonToken()
							)
					)
					.Concat<MemberDeclarationSyntax>
					(
						type.Fields.Select
						(
							field => RequiresSolving(field)
								? SolvableMethod.GenerateSolvingMethod(schema, field)
								: Method.GenerateMethod(field)
						)
					)
			);
			if (type.Name == "Query")
				return result
					.AddMembers
					(
						PropertyDeclaration("Query", "FromDefaultSession")
							.AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
							.AddAccessorListAccessors
							(
								AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken()
							)
							.WithInitializer
							(
								ImplicitObjectCreationExpression()
									.WithInitializer
									(
										AssignmentExpression("Session", ImplicitObjectCreationExpression())
									)
							)
					);

			MethodDeclarationSyntax serializeMethod = MethodDeclaration
			(
				GenericName("ValueTask", "String"),
				"_Serialize"
			)
				.WithExplicitInterfaceSpecifier("ISelfSerializable")
				.WithExpressionBody
				(
					ImplicitObjectCreationExpression
					(
						InvocationExpression
						(
							MemberAccessExpression(InvocationExpression("Id"), "ContinueWith"),
							SimpleLambdaExpression("idTask", MemberAccessExpression("idTask", "Result", "Value"))
						)
					)
				);
			MethodDeclarationSyntax deserializeMethod = MethodDeclaration(IdentifierName(formattedName), "_Deserialize")
				.AddModifiers(SyntaxKind.StaticKeyword)
				.WithExplicitInterfaceSpecifier(GenericName("ISelfDeserializable", formattedName))
				.AddParameterListParameters(Parameter(IdentifierName("String"), "asString"))
				.WithExpressionBody
				(
					InvocationExpression
					(
						MemberAccessExpression("Query", "FromDefaultSession", $"Load{formattedName}FromID"),
						ImplicitObjectCreationExpression(IdentifierName("asString"))
					)
				);
			return result.AddBaseListTypes([GenericName("ISelfDeserializable", formattedName)])
				.AddMembers(serializeMethod, deserializeMethod);
	}
}
