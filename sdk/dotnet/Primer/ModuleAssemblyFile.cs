using System.Runtime.InteropServices;

namespace Dagger.Primer;
using static ModuleAssemblyFile;

readonly record struct AssemblyBuildDirectoryCandidate : IComparable<AssemblyBuildDirectoryCandidate>
{
	public bool IsDebug { get; init; }
	public int FrameworkVersion { get; init; }
	public bool IsPlatformSpecific { get; init; }
	public DateTime LastWriteTime { get; init; }
	public string DllPath { get; init; }


	public int CompareTo(AssemblyBuildDirectoryCandidate other)
	{
		const int preferThis = -1;
		const int preferOther = 1;

		// 1. If other's timestamp is more than 1 second older, sort it after this
		var timeDiff = LastWriteTime - other.LastWriteTime;
		if (timeDiff.TotalSeconds > 10)
			return preferThis;
		if (timeDiff.TotalSeconds < -10)
			return preferOther;

		// 2. Framework version comparison (current → older → newer)
		// "flip" the ordering of versions from 0 to current, so that current -> 0 and 0 -> highest #
		// So if current version is 7, then 7 ranks @ 0, 3 ranks @ 4, and 8 ranks @ 8
		int thisVersionRank = FrameworkVersion > frameworkMajorVersion ? FrameworkVersion : frameworkMajorVersion - FrameworkVersion;
		int otherVersionRank = other.FrameworkVersion > frameworkMajorVersion
			? other.FrameworkVersion
			: frameworkMajorVersion - other.FrameworkVersion;
		if (thisVersionRank != otherVersionRank)
			return thisVersionRank - otherVersionRank;

		// 3. Debug/release preference
		if (IsDebug != other.IsDebug)
			return IsDebug == !preferRelease ? preferThis : preferOther;

		// 4. Platform-specific over platform-agnostic
		if (IsPlatformSpecific != other.IsPlatformSpecific)
			return IsPlatformSpecific ? preferThis : preferOther;

		return 0;
	}
}

// TODO: Detect whether project is uncompiled sln or csproj
class ModuleAssemblyFile
{
	public static readonly int frameworkMajorVersion;
	public static readonly bool preferRelease;
	public static readonly bool isManualPreference;


	public FileInfo File { get; }


	static ModuleAssemblyFile()
	{
		string frameworkDesc = RuntimeInformation.FrameworkDescription;
		int lastSpaceIndex = frameworkDesc.LastIndexOf(' ');
		ReadOnlySpan<char> versionSpan = frameworkDesc.AsSpan(lastSpaceIndex + 1);

		int start = versionSpan[0] == 'v' ? 1 : 0;
		int end = start;
		do ++end; while (end < versionSpan.Length && versionSpan[end] != '.');

		frameworkMajorVersion = int.Parse(versionSpan.Slice(start, end - start));


		string? preferReleaseString = Environment.GetEnvironmentVariable("Dagger:Dotnet:PreferRelease");
		if (preferReleaseString is not null)
		{
			preferRelease = preferReleaseString.Equals("true", StringComparison.OrdinalIgnoreCase) ||
				preferReleaseString.Equals("1", StringComparison.OrdinalIgnoreCase) ||
				preferReleaseString.Equals("yes", StringComparison.OrdinalIgnoreCase);
			if (preferRelease)
				isManualPreference = true;
			else
				isManualPreference = preferReleaseString.Equals("false", StringComparison.OrdinalIgnoreCase) ||
					preferReleaseString.Equals("0", StringComparison.OrdinalIgnoreCase) ||
					preferReleaseString.Equals("no", StringComparison.OrdinalIgnoreCase);
			if (!isManualPreference)
				Console.Error.WriteLine
				(
					$"Warning: Did not understand Env:Dagger:Dotnet:PreferRelease value \"{preferReleaseString}\""
				);
		}
	}

	public ModuleAssemblyFile(string moduleName, string sourcePath)
	{
		var basePath = Path.Combine(sourcePath, moduleName, "bin");
		if (!Directory.Exists(basePath))
			basePath = Path.Combine(sourcePath, "bin");
		if (!Directory.Exists(basePath))
		{
			string? dllPath = Path.Combine(sourcePath, $"{moduleName}.dll");
			if (!System.IO.File.Exists(dllPath))
				dllPath = Directory.EnumerateFiles(sourcePath, $"*.{moduleName}.dll").FirstOrDefault();
			if (dllPath == null)
				throw new Exception("Could not find module assembly in root source.");
			File = new FileInfo(dllPath);
			return;
		}

		List<AssemblyBuildDirectoryCandidate> candidates = new();

		// Collect all viable candidates
		foreach (var isDebug in new[] { true, false })
		{
			var configPath = Path.Combine(basePath, isDebug ? "Debug" : "Release");

			if (!Directory.Exists(configPath))
				continue;

			foreach (var buildDirectoryPath in Directory.EnumerateDirectories(configPath, "net*.0"))
			{
				if (!TryParseBuildDirectoryFrameworkMajorVersion(buildDirectoryPath, out int version))
					continue;

				foreach (string subPath in new[] { $"linux-{RuntimeInformation.ProcessArchitecture.ToString().ToLower()}/", "" })
				{
					var fullPath = Path.Combine(buildDirectoryPath, subPath);
					if (!Directory.Exists(fullPath))
						continue;

					string? dllPath = Path.Combine(fullPath, $"{moduleName}.dll");
					if (!System.IO.File.Exists(dllPath))
						dllPath = Directory.EnumerateFiles(fullPath, $"*.{moduleName}.dll").FirstOrDefault();
					if (dllPath == null)
						continue;

					candidates.Add
					(
						new AssemblyBuildDirectoryCandidate
						{
							IsDebug = isDebug,
							FrameworkVersion = version,
							IsPlatformSpecific = subPath.Length > 0,
							LastWriteTime = System.IO.File.GetLastWriteTime(dllPath),
							DllPath = dllPath
						}
					);
				}
			}
		}

		// Sort candidates according to the specified criteria
		using IEnumerator<AssemblyBuildDirectoryCandidate> enumerator = candidates.Order().GetEnumerator();
		if (!enumerator.MoveNext())
			throw new Exception($"No build found (expected [{moduleName}/]bin/(Debug|Release)/net{frameworkMajorVersion}.0/[linux-{RuntimeInformation.ProcessArchitecture.ToString().ToLower()}/][AssemblyNamespace.]{moduleName}.dll)");

		AssemblyBuildDirectoryCandidate selected = enumerator.Current;

		if (isManualPreference && selected.IsDebug == preferRelease)
		{
			Console.Error.Write
			(
				$"Warning: Selected {(selected.IsDebug ? "debug" : "release")} build despite " +
				$"{(!preferRelease ? "debug" : "release")} preference"
			);
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.IsDebug != selected.IsDebug)
				{
					Console.Error.Write($".  Timestamp or framework version caused {selected.DllPath} to be preferred over {enumerator.Current.DllPath}");
					break;
				}
			}
			Console.Error.WriteLine();
		}
		else
		{
			bool everFoundVersionMismatch = false;
			bool everFoundplatformSpecificMismatch = false;
			while (enumerator.MoveNext())
			{
				AssemblyBuildDirectoryCandidate runnerUp = enumerator.Current;
				bool versionMismatch = runnerUp.FrameworkVersion != selected.FrameworkVersion;
				bool platformSpecificMismatch = runnerUp.IsPlatformSpecific != selected.IsPlatformSpecific;
				if
				(
					(versionMismatch || platformSpecificMismatch) &&
					!everFoundVersionMismatch && !everFoundplatformSpecificMismatch
				)
					Console.Error.Write("Warning");

				if (versionMismatch && !everFoundVersionMismatch)
				{
					everFoundVersionMismatch = true;
					Console.Error.Write
					(
						"  Bin directory contains builds for multiple framework versions."
					);
					if (everFoundplatformSpecificMismatch)
						break;
				}
				if (platformSpecificMismatch && !everFoundplatformSpecificMismatch)
				{
					everFoundplatformSpecificMismatch = true;
					Console.Error.Write
					(
						"  Bin directory contains both Linux and platform-agnostic builds."
					);
					if (everFoundVersionMismatch)
						break;
				}
			}
			if (everFoundplatformSpecificMismatch || everFoundVersionMismatch)
				Console.Error.WriteLine("  Consider using filters, branches, deleting old builds, to reduce disk usage.");
		}

		File = new FileInfo(selected.DllPath);
	}


	private static bool TryParseBuildDirectoryFrameworkMajorVersion(string directoryPath, out int version)
	{
		var versionSpan = directoryPath.AsSpan(directoryPath.LastIndexOf(Path.DirectorySeparatorChar) + 1 + 3 /* "net" */);
		var end = 0;
		while (end < versionSpan.Length && versionSpan[end] != '.')
			end++;

		return int.TryParse(versionSpan.Slice(0, end), out version);
	}
}
