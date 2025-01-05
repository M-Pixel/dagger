namespace Dagger;

/// <summary>See https://docs.dagger.io/api/filters</summary>
[
	AttributeUsage
	(
		AttributeTargets.Assembly|AttributeTargets.Class|AttributeTargets.Struct|AttributeTargets.Constructor|
		AttributeTargets.Method|AttributeTargets.Parameter
	)
]
public sealed class DirectoryFromContextAttribute : Attribute
{
	/// <summary>
	///		When true, inherits <see cref="DefaultPath"/> and/or <see cref="Ignore"/> from the parent function/object if
	///		not specified in this instance of the parameter.  True by default.
	/// </summary>
	public bool Inherit { get; set; } = true;

	/// <summary>
	///		Default path to use if none is specified from the CLI.  Relative to the module or repository root.
	/// </summary>
	public string? DefaultPath { get; init; }

	/// <summary>
	///		Gitignore-style patterns applied to the DefaultPath.
	/// </summary>
	public string[]? Ignore { get; init; }
}

// TODO: Static analysis that warns when used on non-Directory parameter
