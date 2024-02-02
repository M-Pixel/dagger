using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Dagger.Introspection;

record Response
(
	[property: JsonPropertyName("__schema")]
	Schema Schema
);

record Schema
(
	ImmutableArray<Type> Types
);

[JsonConverter(typeof(JsonStringEnumConverter<TypeKind>))]
enum TypeKind
{
	SCALAR,
	OBJECT,
	INTERFACE,
	UNION,
	ENUM,
	INPUT_OBJECT,
	LIST,
	NON_NULL
}

[JsonConverter(typeof(JsonStringEnumConverter<Scalar>))]
enum Scalar
{
	Int,
	Float,
	String,
	Boolean
}

interface IDocumented
{
	string? Description { get; }
}

record Type
(
	TypeKind Kind,
	string Name,
	string? Description, // confirmed nullable
	ImmutableArray<Field> Fields,
	ImmutableArray<InputValue> InputFields,
	ImmutableArray<EnumValue> EnumValues
)
	: IDocumented;

static class TypesExtensions
{
	public static Type? Get(this IEnumerable<Type> self, string name) => self.FirstOrDefault(type => type.Name == name);
}

record Field
(
	string Name,
	string? Description,
	TypeReference Type,
	[property: JsonPropertyName("args")]
	ImmutableArray<InputValue> Arguments,
	bool IsDeprecated,
	string? DeprecationReason,

	Type? ParentObject
)
	: IDocumented;

record TypeReference
(
	TypeKind Kind,
	string? Name,
	TypeReference? OfType
)
{
	public bool IsOptional() => Kind != TypeKind.NON_NULL;
	public bool IsScalar() => (Kind == TypeKind.NON_NULL ? OfType! : this).Kind is TypeKind.SCALAR or TypeKind.ENUM;
	public bool IsObject() => (Kind == TypeKind.NON_NULL ? OfType! : this).Kind == TypeKind.OBJECT;
	public bool IsList() => (Kind == TypeKind.NON_NULL ? OfType! : this).Kind == TypeKind.LIST;

	public TypeKind ResolveKind()
	{
		TypeReference type = this;
		while (type.Name == null)
			type = type.OfType!;
		return type.Kind;
	}

	public string ResolveName()
	{
		TypeReference type = this;
		while (type.Name == null)
			type = type.OfType!;
		return type.Name;
	}
}

record InputValue
(
	string Name,
	string Description,
	string? DefaultValue,
	TypeReference Type
)
	: IDocumented;

record EnumValue
(
	string Name,
	bool IsDeprecated,
	string? Description,
	string? DeprecationReason
)
	: IDocumented;
