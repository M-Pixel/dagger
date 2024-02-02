using System.Collections.Immutable;
using System.Formats.Tar;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Downloader;
using MicroKnights.IO.Streams;
using static System.Environment;
using static System.OperatingSystem;

namespace Dagger;

/// <summary>
/// Runs an engine session from a specified binary
/// </summary>
static class ExecutableDownloader
{
	private const string _CLI_HOST = "dl.dagger.io";
	private const string _DAGGER_CLI_BIN_PREFIX = "dagger";
	private static string? _override_cli_url;
	private static string? _override_checksums_url;
	private static readonly string _cacheDir =
		IsWindows()
			? Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), "Dagger", "Cache")
		: IsMacOS()
			? Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), "Caches", "Dagger")
		: Path.Combine
		(
			GetEnvironmentVariable("XDG_CACHE_HOME") ??
				Path.Combine(GetFolderPath(SpecialFolder.UserProfile), ".cache"),
			"dagger"
		);


	public static async Task<string> DownloadCLI()
	{
		string binPath = BuildBinPath();

		// Create a temporary bin file path
		CreateCacheDir();

		string tmpBinDownloadDir = Path.Combine(_cacheDir, $"temp-{GetRandomId()}");
		if (IsWindows())
			System.IO.Directory.CreateDirectory(tmpBinDownloadDir);
		else
			System.IO.Directory.CreateDirectory
			(
				tmpBinDownloadDir,
				UnixFileMode.UserExecute | UnixFileMode.UserRead | UnixFileMode.UserWrite
			);
		string tmpBinPath = BuildOsExePath(tmpBinDownloadDir, _DAGGER_CLI_BIN_PREFIX);

		try
		{
			// download an archive and use appropriate extraction depending on platforms (zip on windows, tar.gz on other platforms)
			Task<string> actualChecksum = ExtractArchive(tmpBinDownloadDir);
			Task<string> expectedChecksum = ExpectedChecksum();
			if (await actualChecksum != await expectedChecksum)
				throw new Exception($"checksum mismatch: expected {expectedChecksum}, got {actualChecksum}");
			if (!IsWindows())
				System.IO.File.SetUnixFileMode
				(
					tmpBinPath, UnixFileMode.UserExecute | UnixFileMode.UserRead | UnixFileMode.UserWrite
				);
			System.IO.File.Move(tmpBinPath, binPath);
		}
		catch (Exception exception)
		{
			throw new InitEngineSessionBinaryException
			(
				$"failed to download dagger cli binary: {exception}",
				new DaggerExceptionOptions(Cause: exception)
			);
		}
		finally
		{
			System.IO.Directory.Delete(tmpBinDownloadDir, recursive: true);
		}

		// Remove all temporary binary files
		// Ignore current dagger cli or other files that have not be created by this SDK.
		try
		{
			foreach (string filePath in System.IO.Directory.EnumerateFiles(_cacheDir))
			{
				if (filePath == binPath || !Path.GetFileName(filePath).StartsWith(_DAGGER_CLI_BIN_PREFIX))
					continue;

				System.IO.File.Delete(filePath);
			}
		}
		catch (Exception)
		{
			// Log the error but do not interrupt program.
			await Console.Error.WriteLineAsync("could not clean up temporary binary files");
		}

		return binPath;
	}

	/// <summary>Will create a cache directory on user host to store dagger binary.</summary>
	/// <remarks>
	/// If set, it will use envPaths to determine system's cache directory, if not, it will use `$HOME/.cache` as base
	/// path.  Nothing happens if the directory already exists.
	/// </remarks>
	private static void CreateCacheDir()
	{
		if (IsWindows())
			System.IO.Directory.CreateDirectory(_cacheDir);
		else
			System.IO.Directory.CreateDirectory
			(
				_cacheDir,
				UnixFileMode.UserExecute | UnixFileMode.UserRead | UnixFileMode.UserWrite
			);
	}

	/// <summary>Create a path to output dagger cli binary.</summary>
	/// <remarks>
	/// It will store it in the cache directory with a name composed of the base engine session as constant and the
	/// engine identifier.
	/// </remarks>
	private static string BuildBinPath()
	{
		return BuildOsExePath(_cacheDir, $"{_DAGGER_CLI_BIN_PREFIX}-${CLI.VERSION}");
	}

	/// <summary>Create a path to output dagger cli binary.</summary>
	private static string BuildOsExePath(string destinationDir, string filename)
	{
		string binPath = Path.Combine(destinationDir, filename);
		return IsWindows() ? $"{binPath}.exe" : binPath;
	}

	/**
	 * normalizedArch returns the architecture name used by the rest of our SDKs.
	 */
	private static string NormalizedArch()
	{
		string processorArchitecture = RuntimeInformation.OSArchitecture.ToString();
		switch (processorArchitecture)
		{
			case "X64":
				return "amd64";
			default:
				return processorArchitecture;
		}
	}

	/**
	 * normalizedOS returns the os name used by the rest of our SDKs.
	 */
	private static string NormalizedOs()
	{
		if (IsWindows())
			return "windows";
		if (IsAndroid())
			return "android";
		if (IsLinux())
			return "linux";
		if (IsMacOS())
			return "darwin";
		if (IsFreeBSD())
			return "freebsd";
		return "unknown";
	}

	private static string CLIArchiveName()
	{
		if (_override_cli_url != null)
			return Path.GetDirectoryName(new Uri(_override_cli_url).AbsolutePath) ?? "";
		string ext = IsWindows() ? "zip" : "tar.gz";
		return $"dagger_v{CLI.VERSION}_${NormalizedOs()}_${NormalizedArch()}.${ext}";
	}

	private static string CLIArchiveUrl() =>
		_override_cli_url ?? $"https://{_CLI_HOST}/dagger/releases/{CLI.VERSION}/${CLIArchiveName()}";

	private static string CLIChecksumUrl() =>
		_override_checksums_url ?? $"https://{_CLI_HOST}/dagger/releases/{CLI.VERSION}/checksums.txt";

	private static async Task<ImmutableDictionary<string, string>> ChecksumMap()
	{
		// download checksums.txt
		using HttpClient httpClient = new();
		using HttpResponseMessage checksumsResponse = await httpClient.GetAsync(CLIChecksumUrl());
		if (checksumsResponse.IsSuccessStatusCode == false)
			throw new Exception($"failed to download checksums.txt from ${CLIChecksumUrl()}");

		await using Stream checksumsText = await checksumsResponse.Content.ReadAsStreamAsync();
		// iterate over lines filling in map of filename -> checksum
		List<Match> splitLines = new();
		using StreamReader checksumsReader = new(checksumsText);
		Regex splitter = new(@"(.+)\s+(.+)");
		while (await checksumsReader.ReadLineAsync() is string line)
			splitLines.Add(splitter.Match(line));
		return splitLines.ToImmutableDictionary(match => match.Groups[1].Value, match => match.Groups[2].Value);
	}

	private static async Task<string> ExpectedChecksum()
	{
		IReadOnlyDictionary<string, string> checksums = await ChecksumMap();
		if (checksums.TryGetValue(CLIArchiveName(), out string? expectedChecksum) == false)
			throw new Exception($"Failed to find checksum for {CLIArchiveName()} in checksums.txt");
		return expectedChecksum;
	}

	private static async Task<string> ExtractArchive(string destinationDirectory)
	{
		// extract the dagger binary in the cli archive and return the archive of the .zip for windows and .tar.gz for
		// other platforms
		using DownloadService downloader = new();
		Task<Stream> downloadStreamTask = downloader.DownloadFileTaskAsync(CLIArchiveUrl());
		using var hashAlgorithm = SHA256.Create();
		await using Stream downloadStream = await downloadStreamTask;
		using ReadableSplitStream streamSplitter = new(downloadStream);

		Task extractTask;
		if (IsWindows())
		{
			ZipArchive zipArchive = new(streamSplitter.GetForwardReadOnlyStream(), ZipArchiveMode.Read);
			extractTask = new Task
			(
				() =>
				{
					zipArchive.ExtractToDirectory(destinationDirectory, overwriteFiles: true);
					zipArchive.Dispose();
				},
				TaskCreationOptions.LongRunning
			);
		}
		else
		{
			GZipStream gZipStream = new(streamSplitter.GetForwardReadOnlyStream(), CompressionMode.Decompress);
			extractTask = TarFile.ExtractToDirectoryAsync(gZipStream, destinationDirectory, true);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			extractTask.ContinueWith(_ => gZipStream.DisposeAsync());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}

		Task<byte[]> hashTask = hashAlgorithm.ComputeHashAsync(streamSplitter.GetForwardReadOnlyStream());
		await streamSplitter.StartReadAHead();

		string actualChecksum = BitConverter.ToString(await hashTask);
		await extractTask;

		return actualChecksum;
	}

	/// <summary>Generate a timestamp in finest possible resolution</summary>
	static string GetRandomId() => DateTimeOffset.Now.UtcDateTime.Ticks.ToString();


	// Only meant for tests
	public static void _OverrideCLIURL(string url) => _override_cli_url = url;
	public static void _OverrideCLIChecksumsURL(string url) => _override_checksums_url = url;
}
