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
								argument.Type.IsOptional() ? EqualsValueClause(NullLiteralExpression) : null
							)
					)
			);

		if (field.IsDeprecated)
			result = result.AddAttributes(Attribute(IdentifierName("Obsolete")).WithArgument(field.DeprecationReason));

		return result;
	}

	public static IEnumerable<StatementSyntax> AppendQueryTree(Field field)
	{
		// Insert arguments
		bool isForRootClient = field.ParentObject?.Name == "Query";
		foreach
		(
			StatementSyntax statement
			in
			OperationArgumentConversionStatements(field.Arguments, FormatParameterName, isForRootClient)
		)
			yield return statement;

		yield return LocalDeclarationAssignmentStatement
		(
			"_newQueryTree_",
			InvocationExpression(MemberAccessExpression("QueryTree", "Add"))
				.AddArgumentListArguments(LiteralExpression(field.Name), IdentifierName("_arguments_"))
		);
	}

	public static MethodDeclarationSyntax GenerateMethod(Field field)
	{
		TypeSyntax returnType = FormatType(field.Type, isInput: false, forceNonNull: true);
		return MethodDeclaration(returnType, FormatMethodName(field))
			.WithCommon(field)
			.WithBody
			(
				AppendQueryTree(field)
					.Append
					(
						ReturnStatement
						(
							ObjectCreationExpression(returnType)
								.WithInitializer
								(
									new(string, ExpressionSyntax)[]
									{
										("QueryTree", IdentifierName("_newQueryTree_")),
										("Context", IdentifierName("Context"))
									}
								)
						)
					)
			);
	}
}
