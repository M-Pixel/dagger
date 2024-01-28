using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DaggerSDK;

/// <summary>
/// Abstractions that make it possible to use Syntax Tree more concisely and expressively.
/// </summary>
static class SyntaxTree
{
	public static AccessorDeclarationSyntax AddModifiers
	(
		this AccessorDeclarationSyntax self,
		params SyntaxKind[] modifiers
	)
		=> self.WithModifiers(TokenList(self.Modifiers.Concat(modifiers.Select(Token))));

	public static AccessorDeclarationSyntax WithSemicolonToken(this AccessorDeclarationSyntax self)
		=> self.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

	public static ArrayTypeSyntax ArrayType(string identifierName)
		=> SyntaxFactory.ArrayType(IdentifierName(identifierName))
			.AddRankSpecifiers
			(
				ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))
			);

	public static AssignmentExpressionSyntax AssignmentExpression(string identifierName, ExpressionSyntax expression)
		=> SyntaxFactory.AssignmentExpression
		(
			SyntaxKind.SimpleAssignmentExpression,
			IdentifierName(identifierName),
			expression
		);

	public static AttributeSyntax WithArgument(this AttributeSyntax self, string? argument)
		=> argument == null ? self : self.AddArgumentListArguments(AttributeArgument(LiteralExpression(argument)));

	public static BlockSyntax AddVariableDeclarationAssignment
	(
		this BlockSyntax self,
		string name,
		ExpressionSyntax value
	)
		=> self.AddStatements
		(
			LocalDeclarationStatement
			(
				VariableDeclaration(IdentifierName(Token(SyntaxKind.VarKeyword))).WithVariable(name, value)
			)
		);

	public static ClassDeclarationSyntax AddBaseListTypes(this ClassDeclarationSyntax self, params string[] baseNames)
	{
		BaseListSyntax baseList = self.BaseList ?? BaseList();
		return self.WithBaseList
		(
			baseList.WithTypes
			(
				baseList.Types
					.AddRange(baseNames.Select<string, BaseTypeSyntax>(name => SimpleBaseType(IdentifierName(name))))
			)
		);
	}

	public static ClassDeclarationSyntax AddModifiers(this ClassDeclarationSyntax self, params SyntaxKind[] modifiers)
		=> self.WithModifiers(TokenList(self.Modifiers.Concat(modifiers.Select(Token))));

	public static ClassDeclarationSyntax WithMembers
	(
		this ClassDeclarationSyntax self,
		IEnumerable<MemberDeclarationSyntax> members
	)
		=> self.WithMembers(new SyntaxList<MemberDeclarationSyntax>(members));

	public static CollectionExpressionSyntax CollectionExpression(IEnumerable<ExpressionSyntax> expressions)
		=> SyntaxFactory.CollectionExpression
		(
			SeparatedList<CollectionElementSyntax>(expressions.Select(ExpressionElement))
		);

	public static CompilationUnitSyntax AddComment(this CompilationUnitSyntax self, string text)
		=> self.WithLeadingTrivia(self.GetLeadingTrivia().Append(Comment("// " + text)));

	public static CompilationUnitSyntax AddMembers
	(
		this CompilationUnitSyntax self,
		IEnumerable<MemberDeclarationSyntax> members
	)
		=> self.WithMembers(List(self.Members.Concat(members)));

	public static CompilationUnitSyntax AddUsings(this CompilationUnitSyntax self, params string[] usings)
		=> self.AddUsings(usings.Select(fullName => UsingDirective(IdentifierName(fullName))).ToArray());

	public static CompilationUnitSyntax AddUsingStatic(this CompilationUnitSyntax self, params string[] classPath)
		=> self.AddUsings
		(
			UsingDirective(QualifiedName(classPath))
				.WithStaticKeyword(Token(SyntaxKind.StaticKeyword))
		);

	public static CompilationUnitSyntax WithFileScopedNamespaceDeclaration
	(
		this CompilationUnitSyntax self,
		string @namespace
	)
		=> self.AddMembers(FileScopedNamespaceDeclaration(IdentifierName(@namespace)));

	public static ConditionalAccessExpressionSyntax ConditionalAccessExpression
	(
		ExpressionSyntax expression,
		string whenNotNullIdentifierName
	)
		=> SyntaxFactory.ConditionalAccessExpression
		(
			expression,
			MemberBindingExpression(IdentifierName(whenNotNullIdentifierName))
		);

	public static ConditionalAccessExpressionSyntax ConditionalInvocationExpression
	(
		ExpressionSyntax expression,
		string whenNotNullIdentifierName
	)
		=> SyntaxFactory.ConditionalAccessExpression
		(
			expression,
			SyntaxFactory.InvocationExpression(MemberBindingExpression(IdentifierName(whenNotNullIdentifierName)))
		);

	public static EnumDeclarationSyntax AddAttribute
	(
		this EnumDeclarationSyntax self,
		string name,
		params ExpressionSyntax[] arguments
	)
		=> self.AddAttributeLists
		(
			AttributeList().AddAttributes
			(
				Attribute(IdentifierName(name))
					.AddArgumentListArguments(arguments.Select(AttributeArgument).ToArray())
			)
		);

	public static EnumDeclarationSyntax AddModifiers(this EnumDeclarationSyntax self, params SyntaxKind[] modifiers)
		=> self.WithModifiers(TokenList(self.Modifiers.Concat(modifiers.Select(Token))));

	public static EnumDeclarationSyntax WithMembers
	(
		this EnumDeclarationSyntax self,
		IEnumerable<EnumMemberDeclarationSyntax> members
	)
		=> self.WithMembers(SeparatedList(members));

	public static EnumMemberDeclarationSyntax AddAttribute
	(
		this EnumMemberDeclarationSyntax self,
		string name
	)
		=> self.AddAttributeLists(AttributeList(SingletonSeparatedList(Attribute(IdentifierName(name)))));

	public static GenericNameSyntax GenericName(string identifier, params string[] typeArguments)
		=> SyntaxFactory.GenericName
		(
			Identifier(identifier),
			TypeArgumentList(SeparatedList<TypeSyntax>(typeArguments.Select(IdentifierName)))
		);

	public static IEnumerable<XmlNodeSyntax> XmlParagraphs(string documentation)
	{
		string[] paragraphs = documentation.Split("\n\n");
		return paragraphs.Length == 1
			? Enumerable.Repeat(XmlText(paragraphs[0].Replace("\n", " ")), 1)
			: paragraphs.Select(paragraph => XmlParaElement(paragraph.Replace("\n", " ")));
	}

	public static ImplicitObjectCreationExpressionSyntax ImplicitObjectCreationExpression
	(
		params ExpressionSyntax[] arguments
	)
		=> SyntaxFactory.ImplicitObjectCreationExpression(ArgumentList(SeparatedList(arguments.Select(Argument))), null);

	public static ImplicitObjectCreationExpressionSyntax WithInitializer
	(
		this ImplicitObjectCreationExpressionSyntax self,
		ExpressionSyntax expression
	)
		=> self.WithInitializer
		(
			InitializerExpression(SyntaxKind.ObjectInitializerExpression, SingletonSeparatedList(expression))
		);

	public static InvocationExpressionSyntax AddArgumentListArgument
	(
		this InvocationExpressionSyntax self,
		ExpressionSyntax expression
	)
		=> self.AddArgumentListArguments(Argument(expression));

	public static InvocationExpressionSyntax AddArgumentListArgument
	(
		this InvocationExpressionSyntax self,
		string stringLiteral
	)
		=> self.AddArgumentListArguments(Argument(LiteralExpression(stringLiteral)));

	public static InvocationExpressionSyntax AddArgumentListArguments
	(
		this InvocationExpressionSyntax self,
		IEnumerable<ExpressionSyntax> expressions
	)
		=> self.AddArgumentListArguments(expressions.Select(Argument).ToArray());

	public static InvocationExpressionSyntax AddArgumentListArguments
	(
		this InvocationExpressionSyntax self,
		params ExpressionSyntax[] expressions
	)
		=> self.AddArgumentListArguments((IEnumerable<ExpressionSyntax>)expressions);

	public static InvocationExpressionSyntax ChainInvocation
	(
		this InvocationExpressionSyntax self,
		string methodName
	)
		=> SyntaxFactory.InvocationExpression(MemberAccessExpression(self, methodName));

	public static InvocationExpressionSyntax ChainInvocation
	(
		this InvocationExpressionSyntax self,
		string methodName,
		params TypeSyntax[] typeArguments
	)
		=> SyntaxFactory.InvocationExpression
		(
			MemberAccessExpression(self, GenericName(methodName).AddTypeArgumentListArguments(typeArguments))
		);

	public static InvocationExpressionSyntax InvocationExpression(string identifierName)
		=> SyntaxFactory.InvocationExpression(IdentifierName(identifierName));

	public static LiteralExpressionSyntax LiteralExpression(bool value)
		=> SyntaxFactory.LiteralExpression(value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);

	public static LiteralExpressionSyntax LiteralExpression(string value)
		=> SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));

	public static MemberAccessExpressionSyntax MemberAccessExpression
	(
		ExpressionSyntax expression,
		SimpleNameSyntax name
	)
		=> SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, name);

	public static MemberAccessExpressionSyntax MemberAccessExpression(ExpressionSyntax expression, string name)
		=> MemberAccessExpression(expression, IdentifierName(name));

	public static MemberAccessExpressionSyntax MemberAccessExpression(string identifierName, string name)
		=> MemberAccessExpression(IdentifierName(identifierName), name);

	public static MethodDeclarationSyntax AddAttributes
	(
		this MethodDeclarationSyntax self,
		params AttributeSyntax[] attribute
	)
		=> self.AddAttributeLists(AttributeList(SeparatedList(attribute)));

	public static MethodDeclarationSyntax AddModifiers(this MethodDeclarationSyntax self, params SyntaxKind[] tokens)
		=> self.WithModifiers(TokenList(self.Modifiers.Concat(tokens.Select(syntaxKind => Token(syntaxKind)))));

	public static MethodDeclarationSyntax WithExpressionBody
	(
		this MethodDeclarationSyntax self,
		ExpressionSyntax expression
	)
		=> self.WithExpressionBody(ArrowExpressionClause(expression))
			.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

	public static MethodDeclarationSyntax WithParameters
	(
		this MethodDeclarationSyntax self,
		IEnumerable<ParameterSyntax> parameters
	)
		=> self.WithParameterList(ParameterList(SeparatedList(parameters)));

	public static ObjectCreationExpressionSyntax AddArgumentListArguments
	(
		this ObjectCreationExpressionSyntax self,
		params ExpressionSyntax[] expressions
	)
		=> self.AddArgumentListArguments(expressions.Select(Argument).ToArray());

	public static ObjectCreationExpressionSyntax ObjectCreationExpression(string typeName)
		=> SyntaxFactory.ObjectCreationExpression(IdentifierName(typeName));

	public static ObjectCreationExpressionSyntax WithInitializer
	(
		this ObjectCreationExpressionSyntax self,
		IEnumerable<(string, ExpressionSyntax)> members
	)
		=> self.WithInitializer
		(
			InitializerExpression
			(
				SyntaxKind.ObjectInitializerExpression,
				SeparatedList<ExpressionSyntax>
				(
					members.Select(assignment => AssignmentExpression(assignment.Item1, assignment.Item2))
				)
			)
		);

	public static ParameterSyntax Parameter(TypeSyntax type, string text)
		=> SyntaxFactory.Parameter(Identifier(text)).WithType(type);

	public static PropertyDeclarationSyntax AddModifiers
	(
		this PropertyDeclarationSyntax self,
		params SyntaxKind[] modifiers
	)
		=> self.WithModifiers(TokenList(self.Modifiers.Concat(modifiers.Select(Token))));

	public static PropertyDeclarationSyntax PropertyDeclaration(string type, string identifier)
		=> SyntaxFactory.PropertyDeclaration(IdentifierName(type), Identifier(identifier));

	public static PropertyDeclarationSyntax WithInitializer
	(
		this PropertyDeclarationSyntax self,
		ExpressionSyntax expression
	)
		=> self
			.WithInitializer(EqualsValueClause(expression))
			.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

	public static QualifiedNameSyntax QualifiedName(Span<string> classPath)
		=> classPath.Length > 2
			? SyntaxFactory.QualifiedName(QualifiedName(classPath[..^2]), IdentifierName(classPath[^1]))
			: SyntaxFactory.QualifiedName(IdentifierName(classPath[0]), IdentifierName(classPath[1]));

	public static readonly XmlTextSyntax xmlTrailingNewLine =
		XmlText().AddTextTokens(XmlTextNewLine(TriviaList(), "\n", "\n", TriviaList()));

	public static RecordDeclarationSyntax AddModifiers(this RecordDeclarationSyntax self, params SyntaxKind[] modifiers)
		=> self.WithModifiers(TokenList(self.Modifiers.Concat(modifiers.Select(Token))));

	public static RecordDeclarationSyntax RecordDeclaration(string name)
		=> SyntaxFactory.RecordDeclaration(Token(SyntaxKind.RecordKeyword), name)
			.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

	public static RecordDeclarationSyntax WithBody(this RecordDeclarationSyntax self)
		=> self.WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
			.WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));

	public static RecordDeclarationSyntax WithParameters
	(
		this RecordDeclarationSyntax self,
		IEnumerable<ParameterSyntax> parameters
	)
		=> self.WithParameterList(ParameterList(SeparatedList(parameters)));

	public static SimpleLambdaExpressionSyntax SimpleLambdaExpression(string parameterName, ExpressionSyntax expression)
		=> SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(Identifier(parameterName)), expression);

	public static TSyntax AddDocumentationComment<TSyntax>(this TSyntax self, XmlNodeSyntax section)
		where TSyntax : SyntaxNode
		=> self.AddDocumentationComments(new[]{section});

	public static TSyntax AddDocumentationComments<TSyntax>
	(
		this TSyntax self,
		IEnumerable<XmlNodeSyntax> sections
	) where TSyntax : SyntaxNode
		=> self.WithLeadingTrivia
		(
			self.GetLeadingTrivia()
				.Prepend
				(Trivia(
					DocumentationCommentTrivia
						(
							SyntaxKind.MultiLineDocumentationCommentTrivia,
							List(sections.SelectMany(section => new[]{ section.WithLeadingTrivia(DocumentationCommentExterior("/// ")), xmlTrailingNewLine }))
						)
				))
		);

	public static TypeSyntax Nullable(TypeSyntax type)
		=> type is NullableTypeSyntax ? type : NullableType(type);

	public static VariableDeclarationSyntax WithVariable
	(
		this VariableDeclarationSyntax self,
		string name,
		ExpressionSyntax value
	)
		=> self.AddVariables(VariableDeclarator(name).WithInitializer(EqualsValueClause(value)));

	public static XmlElementSyntax XmlParaElement(string content) => SyntaxFactory.XmlParaElement(XmlText(content));

	public static XmlElementSyntax XmlParamElement(string paramName, IEnumerable<XmlNodeSyntax> content)
		=> SyntaxFactory.XmlParamElement(paramName, List(content));

	public static XmlElementSyntax XmlSummaryElement(IEnumerable<XmlNodeSyntax> content)
		=> SyntaxFactory.XmlSummaryElement(List(content));
}
