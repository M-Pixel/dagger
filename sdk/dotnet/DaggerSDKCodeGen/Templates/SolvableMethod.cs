using DaggerSDK.Introspection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static DaggerSDK.Functions;
using static DaggerSDK.SyntaxTree;

namespace DaggerSDK;

static class SolvableMethod
{
	/// <summary>Generate solver method that returns a <see cref="Task"/>.</summary>
	public static MethodDeclarationSyntax GenerateSolvingMethod(Schema schema, Field field)
	{
		bool convertsId = ConvertsID(field);
		BlockSyntax body = Block();

		// If it's a scalar, make it possible to return its already-filled value.
		if (field.Type.IsScalar() && field.ParentObject!.Name != "Query" && convertsId == false)
		{
			var cacheFieldReference = IdentifierName("Cached" + FormatName(field.Name));
			body = body.AddStatements
			(
				IfStatement
				(
					BinaryExpression
					(
						SyntaxKind.NotEqualsExpression,
						cacheFieldReference,
						LiteralExpression(SyntaxKind.NullLiteralExpression)
					),
					ReturnStatement
					(
						field.Type.ResolveName() == nameof(Scalar.String) || IsCustomScalar(field.Type)
							? cacheFieldReference
							: MemberAccessExpression
							(
								SyntaxKind.SimpleMemberAccessExpression,
								cacheFieldReference,
								IdentifierName("Value")
							)
					)
				)
			);
		}

		body = body.AddStatements(Method.AppendQueryTree(field));

		// Add subfields
		if (field.Type.IsList() && IsListOfObject(field.Type))
			body = body.AddStatements
			(
				ExpressionStatement
				(
					AssignmentExpression
					(
						"_newQueryTree_",
						InvocationExpression(MemberAccessExpression("_newQueryTree_", "Add"))
							.AddArgumentListArgument
							(
								string.Join(' ', GetArrayField(schema, field).Select(arrayField => arrayField.Name))
							)
					)
				)
			);

		AwaitExpressionSyntax awaitComputeExpression = AwaitExpression
		(
			InvocationExpression("ComputeQuery")
				.AddArgumentListArguments
				(
					IdentifierName("_newQueryTree_"),
					AwaitExpression(InvocationExpression(MemberAccessExpression("Context", "Connection")))
				)
		);

		TypeSyntax returnType = FormatType(field.Type, isInput: convertsId);

		body = convertsId
			? body.AddStatements
			(
				ExpressionStatement(awaitComputeExpression),
				ReturnStatement(ThisExpression())
			)

			: !(field.Type.IsList() && IsListOfObject(field.Type))
			? body.AddStatements
			(
				ReturnStatement
				(
					IsCustomScalar(field.Type)
						? ImplicitObjectCreationExpression
						(
							DeserializeScalarStatement
							(
								awaitComputeExpression,
								PredefinedType(Token(SyntaxKind.StringKeyword))
							)
						)
						: DeserializeScalarStatement(awaitComputeExpression, returnType)
				)
			)

			: body.AddStatements
			(
				ReturnStatement
				(
					InvocationExpression
					(
						MemberAccessExpression(
							ParenthesizedExpression(awaitComputeExpression),
							IdentifierName("EnumerateArray")
						)
					)
					.ChainInvocation("Select")
					.AddArgumentListArgument
					(
						SimpleLambdaExpression
						(
							"json",
							ObjectCreationExpression
							(
								FormatType(GetArrayType(field), isInput: false, forceNonNull: true)
							)
								.WithInitializer
								(
									new(string, ExpressionSyntax)[]
									{
										("QueryTree", IdentifierName("QueryTree")),
										("Context", IdentifierName("Context"))
									}
									.Concat
									(
										GetArrayField(schema, field).Select<Field, (string, ExpressionSyntax)>
										(
											innerField =>
											(
												"Cached" + FormatName(innerField.Name),
												InvocationExpression(MemberAccessExpression("json", "GetProperty"))
													.AddArgumentListArgument(innerField.Name)
													.ChainInvocation
													(
														"Deserialize",
														FormatType(innerField.Type, isInput: false)
													)
											)
										)
									)
								)
							)
					)
					.ChainInvocation("ToImmutableArray")
				)
			);

		return MethodDeclaration
			(
				// Write return type
				GenericName(Identifier("Task")).AddTypeArgumentListArguments(returnType),
				FormatMethodName(field)
			)
			.WithCommon(field)
			.AddModifiers(Token(SyntaxKind.AsyncKeyword))
			.WithBody(body);
	}

	private static ExpressionSyntax DeserializeScalarStatement
	(
		ExpressionSyntax computeExpression,
		TypeSyntax returnType
	)
		=> InvocationExpression
			(
				MemberAccessExpression
				(
					ParenthesizedExpression(computeExpression),
					GenericName(Identifier("Deserialize")).AddTypeArgumentListArguments(returnType)
				)
			);
}
