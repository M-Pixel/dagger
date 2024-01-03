namespace DaggerSDKCodeGen.Models;

record QueryArg(
	string? DefaultValue,
	string? Description,
	string? Name,
	ArgType? Type
);
