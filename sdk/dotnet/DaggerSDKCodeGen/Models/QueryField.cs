using System.Collections.Immutable;

namespace DaggerSDKCodeGen.Models;

record RawQueryField(
	ImmutableArray<QueryArg>? Args,
	string? DeprecationReason,
	string? Description,
	bool IsDeprecated,
	string? Name,
	ArgType? Type
);

record QueryField : RawQueryField
{
	public readonly QueryType ParentType;

	public QueryField(RawQueryField raw, QueryType parentType)
		: base(raw)
	{
		ParentType = parentType;
	}
}
