using System.Runtime.InteropServices;

namespace Dagger.Runtime;

readonly record struct AssemblyBuildDirectoryCandidate(string Path, bool IsDebug, int Version);

record AssemblyStreamSet(Stream Bytecode, Stream? Symbols, Stream? Documentation) : IDisposable
{
	public static AssemblyStreamSet Locate(string moduleName, string sourceSubpath)
	{
		string? preferReleaseString = Environment.GetEnvironmentVariable("Dagger:Dotnet:PreferRelease");
		bool preferRelease = preferReleaseString is not null &&
			(preferReleaseString.Equals("true", StringComparison.OrdinalIgnoreCase) ||
			 preferReleaseString.Equals("1", StringComparison.OrdinalIgnoreCase) ||
			 preferReleaseString.Equals("yes", StringComparison.OrdinalIgnoreCase));

		var basePath = Path.Combine("/mnt/module", sourceSubpath, moduleName, "bin");
		if (!System.IO.Directory.Exists(basePath))
			basePath = Path.Combine("/mnt/module", sourceSubpath, "bin");

		AssemblyBuildDirectoryCandidate? bestMatch = null;
		int currentFrameworkVersion = GetCurrentFrameworkMajorVersion();

		foreach (var isDebug in new[]{ !preferRelease, preferRelease })
		{
			var configPath = Path.Combine(basePath, isDebug ? "Debug" : "Release");

			if (!System.IO.Directory.Exists(configPath))
				continue;

			foreach (var buildDirectoryPath in System.IO.Directory.EnumerateDirectories(configPath, "net*.0"))
			{
				if (!TryParseBuildDirectoryFrameworkMajorVersion(buildDirectoryPath, out int version))
					continue;

				if
				(
					!bestMatch.HasValue
					|| (version <= currentFrameworkVersion && version > bestMatch.Value.Version)
					|| (version > currentFrameworkVersion && version < bestMatch.Value.Version)
				)
					bestMatch = new AssemblyBuildDirectoryCandidate(buildDirectoryPath, isDebug, version);
			}
			if (bestMatch.HasValue && bestMatch.Value.Version == currentFrameworkVersion)
				break;
		}

		if (!bestMatch.HasValue)
			throw new Exception($"No build directory found (expected [{moduleName}/]bin/(Debug|Release)/net#.0)");

		string? dllPath = null;
		foreach (string subPath in new[]{ $"linux-{RuntimeInformation.ProcessArchitecture.ToString().ToLower()}/", "" })
		{
			if (!System.IO.Directory.Exists(bestMatch.Value.Path + '/' + subPath))
				continue;
			dllPath = System.IO.Directory.EnumerateFiles(bestMatch.Value.Path, $"{subPath}*.{moduleName}.dll")
				.FirstOrDefault();
			if (dllPath != null)
				break;

			dllPath = Path.Combine(bestMatch.Value.Path, $"{subPath}{moduleName}.dll");
			if (System.IO.File.Exists(dllPath))
				break;

			dllPath = null;
		}
		if (dllPath == null)
			throw new Exception
			(
				$"No assembly found in {bestMatch.Value.Path} (expected [linux-" +
				$"{RuntimeInformation.ProcessArchitecture.ToString().ToLower()}/][AssemblyNamespace.]{moduleName}.dll)"
			);

		FileStream bytecodeStream = new(dllPath, FileMode.Open, FileAccess.Read, FileShare.Read);

		string symbolsFilePath = dllPath[..^3] + "pdb";
		FileStream? symbolsStream = System.IO.File.Exists(symbolsFilePath)
			? new(symbolsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)
			: null;

		string documentationFilePath = dllPath[..^3] + "xml";
		FileStream? documentationStream = System.IO.File.Exists(documentationFilePath)
			? new(documentationFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)
			: null;

		return new AssemblyStreamSet(bytecodeStream, symbolsStream, documentationStream);
	}

	~AssemblyStreamSet() => Dispose();

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Bytecode.Dispose();
		Symbols?.Dispose();
	}

	private static int GetCurrentFrameworkMajorVersion()
	{
		string frameworkDesc = RuntimeInformation.FrameworkDescription;
		int lastSpaceIndex = frameworkDesc.LastIndexOf(' ');
		ReadOnlySpan<char> versionSpan = frameworkDesc.AsSpan(lastSpaceIndex + 1);

		int start = versionSpan[0] == 'v' ? 1 : 0;
		int end = start;
		do ++end; while (end < versionSpan.Length && versionSpan[end] != '.');

		return int.Parse(versionSpan.Slice(start, end - start));
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
