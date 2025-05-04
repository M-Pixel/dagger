using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dagger;
using static Dagger.Alias;

public static partial class DevelopmentTimeTasks
{
	private static Task<string> DaggerVerion =>
		_daggerVersion ??= DAG.Version().ContinueWith(versionTask => versionTask.Result[1..]); // Remove the "v" prefix
	[JsonIgnore] private static Task<string>? _daggerVersion;

	public static async Task<Directory> Bump
	(
		[DirectoryFromContext(DefaultPath = "/sdk/dotnet/Client")]
		Directory source,
		string version
	)
	{
		return DAG.GetDirectory()
			.WithDirectory("sdk/dotnet/Client", await UpdatePackageVersion(source, version.TrimStart('v')));
	}

	internal static async Task<Directory> UpdatePackageVersion(Directory clientSource, string version)
	{
		string csproj = await clientSource.File("Client.csproj").Contents();
		Regex versionReplacer = new("<Version>.*</Version>");
		return DAG.GetDirectory().WithNewFile
		(
			"Client.csproj",
			versionReplacer.Replace(csproj, $"<Version>{version}</Version>", 1)
		);
	}
}
