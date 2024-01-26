using DaggerSDK.Introspection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static DaggerSDK.Functions;
using static DaggerSDK.SyntaxTree;

namespace DaggerSDK;

static class Classes
{
	public static IEnumerable<MemberDeclarationSyntax> Generate(Schema schema)
		=> schema.Types
			.Where(type => type.Fields.Length > 0 && type.Name.StartsWith("__") == false)
			.Select(type => GenerateObject(schema, type));

	static MemberDeclarationSyntax GenerateObject(Schema schema, Introspection.Type type)
	{
		string formattedName = FormatName(type.Name);
		ClassDeclarationSyntax result = ClassDeclaration(formattedName)
			.AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword)
			.AddDocumentationComments(type)
			.AddBaseListTypes("BaseClient")
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
						PropertyDeclaration("Client", "Default")
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
										AssignmentExpression("Context", MemberAccessExpression("Context", "Default"))
									)
							)
					);
			return result;
	}
}
