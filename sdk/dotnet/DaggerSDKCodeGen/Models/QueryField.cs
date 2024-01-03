using System.Collections.Immutable;

namespace DaggerSDKCodeGen.Models;

record QueryField(
	ImmutableArray<QueryArg>? Args,
	string? DeprecationReason,
	string? Description,
	bool IsDeprecated,
	string? Name,
	ArgType? Type
);
