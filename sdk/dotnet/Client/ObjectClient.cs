using System.Collections.Immutable;
using System.Text.Json;

namespace Dagger;

/// <summary>
/// Client-side representation of a Dagger object.
/// </summary>
/// <remarks>Analogous to "Base Client" in other SDK implementations.</remarks>
public abstract class ObjectClient
{
	internal Session Session { get; init; } = new();
	internal ImmutableList<Operation> QueryTree { get; init; } = ImmutableList<Operation>.Empty;


	internal async Task<string> Compute()
	{
		JsonElement jsonResult = await APIUtils.ComputeQuery(QueryTree.Add("id"), Session.AcquireGraphQLClient());
		return jsonResult.Deserialize<string>()!;
	}
}
