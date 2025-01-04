namespace Dagger;

/// <summary>See https://docs.dagger.io/api/filters</summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class DirectoryFromContextAttribute : Attribute
{
	/// <summary>
	/// Default path to use if none is specified from the CLI.  Relative to the module or repository root.
	/// </summary>
	public string? DefaultPath { get; init; }

	/// <summary>
	/// Gitignore-style patterns applied to the DefaultPath.
	/// </summary>
	public string[]? Ignore { get; init; }
}

// TODO: Static analysis that warns when used on non-Directory parameter
