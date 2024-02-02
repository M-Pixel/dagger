using System.Collections.Immutable;
using Dagger.Introspection;

namespace Dagger;

class CSharpGenerator : Generator
{
	public CSharpGenerator(Configuration configuration) {}

	/// <summary>
	/// Will generate the C# SDK code and might modify the schema to reorder types in a alphanumeric fashion.
	/// </summary>
	public override GeneratedState Generate(Schema schema)
	{
		schema = schema with
		{
			Types = schema.Types
				.Select
				(
					type => type with
					{
						Fields = type.Fields
							.OrderBy(queryField => queryField.Name, FieldSorter.instance)
							.ToImmutableArray()
					}
				)
				.OrderBy(queryType => queryType.Name)
				.ToImmutableArray()
		};

		return new GeneratedState
		(
			Overlay: API.Generate(schema)
		);
	}

	class FieldSorter : IComparer<string?>
	{
		public static readonly FieldSorter instance = new();

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
}
