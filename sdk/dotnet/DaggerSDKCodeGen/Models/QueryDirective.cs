using System.Collections.Immutable;

namespace DaggerSDKCodeGen.Models;

record QueryDirective(
	QueryArg[]? Args,
	string? Description,
	ImmutableArray<string>? Locations,
	string? Name
);
