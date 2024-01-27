using System.Collections.Immutable;

namespace DaggerSDK;

/// <summary>Instructs the query resolver how to serialize the accompanying value.</summary>
enum ParameterSerialization : byte
{
	/// <summary>Value is a string with complex characters - surround with quotes and escape.</summary>
	String,

	/// <summary>Value is a nested array of <see cref="OperationArgument"/>s - surround with {} and recurse.</summary>
	Object,

	/// <summary>Value has a simple representation that should not be demarcated.</summary>
	Enum,

	/// <summary>Value is a <see cref="Client"/> object that needs to be substituted with an ID string.</summary>
	Reference
}

/// <summary>User-provided argument to a query, with hints about how to serialize it.</summary>
/// <param name="Name">GraphQL uses explicitly named, not positional, arguments.</param>
/// <param name="Value">When null, represents the absence of this parameter.</param>
/// <param name="HowToSerialize">Forms an unambiguous contract between the client function and the resolver.</param>
/// <param name="IsArray">
///		Compliments <paramref name="HowToSerialize"/>, indicating whether Value is an array or single of that thing.
/// </param>
/// <remarks>
///		While possible to determine "HowToSerialize" by analyzing the type of Value through reflection, the code for
///		doing so is substantially more complicated, and less efficient.  The efficiency hardly matters, but that's
///		exactly why eliminating a few bytes isn't worth the resulting addition in complexity and ambiguity elsewhere.
/// </remarks>
record struct OperationArgument
(
	string Name,
	object? Value,
	ParameterSerialization HowToSerialize,
	bool IsArray
)
{
	public OperationArgument()
		: this("", null, ParameterSerialization.String, false)
	{}
}

record Operation
(
	string Name,
	OperationArgument[] Arguments
)
{
	public Operation(string Name)
		: this(Name, [])
	{}
}

static class QueryTreeExtensions
{
	public static ImmutableList<Operation> Add
	(
		this ImmutableList<Operation> queryTree,
		string operationName,
		params OperationArgument[] arguments
	)
		=> queryTree.Add(new Operation(operationName, arguments));
}

public abstract class BaseClient
{
	internal ImmutableList<Operation> QueryTree { get; init; } = ImmutableList<Operation>.Empty;
	internal Context Context { private protected get; init; } = new();
}
