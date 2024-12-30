using Dagger.Introspection;

namespace Dagger.Generator;

static class SchemaExtensions
{
	public static Schema SetParents(this Schema schema) => schema with
	{
		Types =
		[
			..schema.Types
				.Select
				(
					type => type with
					{
						Fields = [..type.Fields.Select(field => field with { ParentObject = type })]
					}
				)
		]
	};
}
