using Dagger.Primer;
// ReSharper disable NotResolvedInText

string moduleName = Environment.GetEnvironmentVariable("Dagger:Module:Name")
	?? throw new ArgumentNullException("Dagger:Module:Name");
string sourcePath = Environment.GetEnvironmentVariable("Dagger:Module:SourcePath")
	?? throw new ArgumentNullException("Dagger:Module:SourcePath");

ModuleAssemblyFile moduleAssemblyFile = new(moduleName, sourcePath);

string depsPath = moduleAssemblyFile.File.FullName[..^3] + "deps.json";
await NuGetClient.Restore(sourcePath, depsPath);

await File.WriteAllTextAsync("/etc/dagger/AssemblyPath", moduleAssemblyFile.File.FullName);
