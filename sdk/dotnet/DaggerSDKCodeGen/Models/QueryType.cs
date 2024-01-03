using System.Collections.Immutable;

namespace DaggerSDKCodeGen.Models;

record QueryType(
	string? Description,
	ImmutableArray<EnumType>? EnumValues,
	ImmutableArray<QueryField>? Fields,
	ImmutableArray<InputField>? InputFields,
	ImmutableArray<string>? Interfaces,
	string? Kind,
	string? Name,
	ImmutableArray<string>? PossibleTypes
);
