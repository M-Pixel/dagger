namespace DaggerSDKCodeGen.Models;

record EnumType(
	string? DeprecationReason,
	string? Description,
	bool IsDeprecated,
	string? Name
);
