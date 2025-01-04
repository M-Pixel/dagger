using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;

namespace Dagger;

public static class ClientCompiler
{
	public static void Compile(CompilationUnitSyntax unit)
	{
		const string referenceAssemblies = "/usr/share/dotnet/packs/Microsoft.NETCore.App.Ref/";
		string versionSpecificSubpath = RuntimeInformation.FrameworkDescription[5..];
		string targetSpecificSubpath = Assembly.GetExecutingAssembly().GetCustomAttribute<TargetFrameworkAttribute>()!
			.FrameworkDisplayName![5..];

		IEnumerable<MetadataReference> references = Directory
			.EnumerateFiles
			(
				Environment.GetEnvironmentVariable("Dagger:Module:Name") != null
					? "/mnt/Client"
					: "Client/bin/Release/net8.0"
			)
			.Where(file => file.EndsWith(".dll"))
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
				.Select
				(
					assembly =>
						$"{referenceAssemblies}/{versionSpecificSubpath}/ref/net{targetSpecificSubpath}/{assembly}.dll"
				)
			)
			.Select(dllPath => MetadataReference.CreateFromFile(dllPath));

		var compilation = CSharpCompilation.Create
		(
			"Dagger.Generated",
			[unit.SyntaxTree],
			references,
			new CSharpCompilationOptions
			(
				OutputKind.DynamicallyLinkedLibrary,
				optimizationLevel: OptimizationLevel.Release,
				nullableContextOptions: NullableContextOptions.Enable
			)
		);

		Directory.CreateDirectory("Libraries");
		using FileStream bytecodeStream =
			new("Libraries/Dagger.Generated.dll", FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		using FileStream symbolsStream =
			new("Libraries/Dagger.Generated.pdb", FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		using FileStream documentationStream =
			new("Libraries/Dagger.Generated.xml", FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		EmitResult result = compilation.Emit(bytecodeStream, symbolsStream, documentationStream);
		if (!result.Success)
			throw new Exception(string.Join('\n', result.Diagnostics.Select(diagnostic => diagnostic.GetMessage())));

	}
}
