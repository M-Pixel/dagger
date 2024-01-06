using System.Collections.Immutable;

namespace DaggerSDKCodeGen.Models;

record RawQueryType(
	string? Description,
	ImmutableArray<EnumType>? EnumValues,
	ImmutableArray<RawQueryField>? Fields,
	ImmutableArray<InputField>? InputFields,
	ImmutableArray<string>? Interfaces,
	string? Kind,
	string? Name,
	ImmutableArray<string>? PossibleTypes
);

class FieldSorter : IComparer<string?>
{
	public static readonly FieldSorter Instance = new();

	private FieldSorter() {}

	public int Compare(string? x, string? y)
	{
		if (x == "id" && y == "id")
			return 1;
		if (x == "id")
			return -1;
		if (y == "id")
			return 1;
		return String.CompareOrdinal(x, y);
	}
}

record QueryType
{
	public readonly string? Description;
	public readonly ImmutableArray<EnumType>? EnumValues;
	public readonly ImmutableArray<QueryField>? Fields;
	public readonly ImmutableArray<InputField>? InputFields;
	public readonly ImmutableArray<string>? Interfaces;
	public readonly string? Kind;
	public readonly string? Name;
	public readonly ImmutableArray<string>? PossibleType;

	public QueryType(RawQueryType raw)
	{
		Description = raw.Description;
		EnumValues = raw.EnumValues;
		Fields = raw.Fields?
			.Select(rawField => new QueryField(rawField, this))
			.OrderBy(queryField => queryField.Name, FieldSorter.Instance)
			.ToImmutableArray();
		InputFields = raw.InputFields;
		Interfaces = raw.Interfaces;
		Kind = raw.Kind;
		Name = raw.Name;
		PossibleType = raw.PossibleTypes;
	}
}
