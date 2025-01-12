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

		string? moduleName = Environment.GetEnvironmentVariable("Dagger:Module:Name");
		string clientPath = moduleName != null ? "/mnt/Client" : "Client/bin/Release/net8.0";
		string generatedAssemblyName = "Dagger.Generated";

		IEnumerable<MetadataReference> references = Directory
			.EnumerateFiles(clientPath)
			// .Where(path => path.EndsWith(".dll") && !path.EndsWith("/Dagger.Client.dll"))
			.Where(path => path.EndsWith(".dll"))
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
}
