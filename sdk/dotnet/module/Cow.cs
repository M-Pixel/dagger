using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dagger;
using static Dagger.Alias;

/// <summary>A Dagger Module Object that is going places.</summary>
public class Cow : IJsonOnDeserialized
{
	/// <summary>Specifies roughly where the message should be wrapped.</summary>
	public int Wrap;

	/// <summary>The appearance of the cow's eyes.</summary>
	public string Eyes;

	public bool Shout;

	/// <summary>The appearance of the cow's tongue.</summary>
	private readonly string _tongue;

	// A non-serializable property.
	[JsonIgnore]
	private HttpClient _httpClient = new();


	public Cow(string eyes, string? tongue, int wrap = 50)
	{
		_tongue = tongue ?? "";
		Eyes = eyes;
		Wrap = wrap;
	}


	public static Cow DefaultCow() => new("^^", null);

	public static Container CowContainer() => DAG.Container().From("docker.io/rancher/cowsay");


	public Task<string[]> Say(IEnumerable<string> thingsToSay) => Task.WhenAll
	(
		thingsToSay.Select
		(
			thing => CowContainer()
				.WithExec(ApplyTongue(["-W" + Wrap, "-e" + Eyes, ApplyShout(thing)]), useEntrypoint: true)
				.Stdout()
		)
	);

	private string ApplyShout(string thing) => Shout ? thing.ToUpperInvariant() : thing;

	private IEnumerable<string> ApplyTongue(IEnumerable<string> command)
		=> string.IsNullOrWhiteSpace(_tongue) ? command : command.Prepend("-T" + _tongue);

	void IJsonOnDeserialized.OnDeserialized()
	{
		_httpClient = new();
	}
}
