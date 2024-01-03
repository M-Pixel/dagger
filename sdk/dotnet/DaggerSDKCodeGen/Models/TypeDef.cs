namespace DaggerSDKCodeGen.Models;

record TypeDef(
	string? Kind,
	string? Name,
	TypeDef? OfType
);
