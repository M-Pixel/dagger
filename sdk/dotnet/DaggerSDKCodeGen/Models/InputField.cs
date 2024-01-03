namespace DaggerSDKCodeGen.Models;

record InputField(
	string? DefaultValue,
	string? Description,
	string? Name,
	ArgType? Type
);