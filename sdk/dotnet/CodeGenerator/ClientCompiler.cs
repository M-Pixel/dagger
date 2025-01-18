using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;

namespace Dagger;

public static class ClientCompiler
{
	public static void Compile(CompilationUnitSyntax unit)
	{
		string? moduleName = Environment.GetEnvironmentVariable("Dagger:Module:Name");
		// Code Generator declares Dagger.Client as a dependency so that Primer will download it (and its dependencies)
		// from NuGet and symlink them into the /CodeGenerator directory.  The generated code will link against
		// these.
		string clientPath = moduleName != null ? "/CodeGenerator" : "Client/bin/Release/net8.0";
		string referenceAssembliesPath = moduleName != null ? "/Reference/" : GetReferenceAssembliesPath();
		string generatedAssemblyName = "Dagger.Generated";

		IEnumerable<MetadataReference> references = Directory
			.EnumerateFiles(clientPath)
			.Where(path => path.EndsWith(".dll") && !path.EndsWith("/Dagger.CodeGenerator.dll"))
			.Concat
			(
				new[]
				{
					"System.Collections",
					"System.Collections.Immutable",
					"System.Runtime",
					"System.Text.Json",
					"System.Linq"
				}
				.Select(assembly => $"{referenceAssembliesPath}{assembly}.dll")
			)
			.Select(dllPath => MetadataReference.CreateFromFile(dllPath));

		var compilation = CSharpCompilation.Create
		(
			generatedAssemblyName,
			[unit.SyntaxTree],
			references,
			new CSharpCompilationOptions
			(
				OutputKind.DynamicallyLinkedLibrary,
				optimizationLevel: OptimizationLevel.Release,
				nullableContextOptions: NullableContextOptions.Annotations
			)
		);

		Directory.CreateDirectory("Generated");
		using FileStream bytecodeStream =
			new($"Generated/{generatedAssemblyName}.dll", FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		using FileStream symbolsStream =
			new($"Generated/{generatedAssemblyName}.pdb", FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		using FileStream documentationStream =
			new($"Generated/{generatedAssemblyName}.xml", FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		EmitResult result = compilation.Emit(bytecodeStream, symbolsStream, documentationStream);
		if (!result.Success)
		{
			string sourceCode = compilation.SyntaxTrees[0].ToString();
			throw new Exception
			(
				string.Join
				(
					"\n\n",
					result.Diagnostics
						.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
						.Select(diagnostic => diagnostic.ToString())
						.Append(sourceCode)
				)
			);
		}
	}

	/// <summary>Constructs the path under which reference assemblies can be found on the current host.</summary>
	private static string GetReferenceAssembliesPath()
	{
		string coreLibraryLocation = typeof(object).Assembly.Location;
		ReadOnlySpan<char> dotnetLocation =
			coreLibraryLocation.AsSpan()[..(coreLibraryLocation.IndexOf("dotnet/", StringComparison.Ordinal) + 7)];
		// FrameworkDescription gives something like ".NET 8.0.11"
		ReadOnlySpan<char> fullVersion = RuntimeInformation.FrameworkDescription.AsSpan()[5..];
		ReadOnlySpan<char> majorMinorVersion = fullVersion[..fullVersion.LastIndexOf('.')];
		return $"{dotnetLocation}packs/Microsoft.NETCore.App.Ref/{fullVersion}/ref/net{majorMinorVersion}/";
	}
}
