using DaggerSDK.Introspection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static DaggerSDK.Functions;
using static DaggerSDK.SyntaxTree;

namespace DaggerSDK;

static class Method
{
	/// <summary>Places required fields before optional fields.</summary>
	class ArgumentComparer : IComparer<InputValue>
	{
		public int Compare(InputValue? x, InputValue? y)
		{
			if (x == y)
				return 0;
			if (x == null || x.Type.IsOptional() && y!.Type.IsOptional() == false)
				return 1;
			if (y == null || y.Type.IsOptional() && x.Type.IsOptional() == false)
				return -1;
			return 0;
		}
	}

	private static readonly ArgumentComparer _argumentComparer = new();

	public static MethodDeclarationSyntax WithCommon(this MethodDeclarationSyntax methodDeclaration, Field field)
	{
		MethodDeclarationSyntax result = methodDeclaration
			.AddModifiers(SyntaxKind.PublicKeyword)
			// Write method comment
			.AddDocumentationComments(field)
			.WithParameters
			(
				field.Arguments
					// Write required arguments before optional arguments
					.Order(_argumentComparer)
					.Select
					(
						argument => Parameter
						(
							FormatType
							(
								argument.Type,
								isInput: !(argument.Name == "id" && field.ParentObject?.Name == "Query")
							),
							FormatParameterName(argument.Name)
						)
							.WithDefault
							(
								argument.Type.IsOptional()
									? EqualsValueClause(LiteralExpression(SyntaxKind.DefaultLiteralExpression))
									: null
							)
					)
			);

		if (field.IsDeprecated)
			result = result.AddAttributes(Attribute(IdentifierName("Obsolete")).WithArgument(field.DeprecationReason));

		return result;
	}

	public static InvocationExpressionSyntax AppendQueryTree(Field field)
	{
		// Insert arguments
		bool isForRootClient = field.ParentObject?.Name == "Query";
		InvocationExpressionSyntax queryAddChain =
			InvocationExpression(MemberAccessExpression("QueryTree", "Add"))
			.AddArgumentListArguments
			(
				OperationArgumentConversionExpressions(field.Arguments, FormatParameterName, isForRootClient)
					.Prepend(LiteralExpression(field.Name))
			);

		return queryAddChain;
	}

	public static MethodDeclarationSyntax GenerateMethod(Field field)
	{
		TypeSyntax returnType = FormatType(field.Type, isInput: false, forceNonNull: true);
		return MethodDeclaration(returnType, FormatMethodName(field))
			.WithCommon(field)
			.AddBodyStatements
			(
				ReturnStatement
				(
					ObjectCreationExpression(returnType)
						.WithInitializer
						(
							new(string, ExpressionSyntax)[]
							{
								("QueryTree", AppendQueryTree(field)),
								("Context", IdentifierName("Context"))
							}
						)
				)
			);
	}
}
