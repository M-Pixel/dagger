using Dagger.Primer;
using NuGet.Versioning;


bool localTest = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true";
string primedStatePath = "/PrimedState/";
if (localTest)
	primedStatePath = '.' + primedStatePath;


string? programPackageName = Environment.GetEnvironmentVariable("Dagger:Primer:InstallPackage");
if (programPackageName != null)
{
	int vIndex = programPackageName.IndexOf('v');
	string versionString = programPackageName[(vIndex + 1)..];
	if (NuGetVersion.TryParse(versionString, out var version))
	{
		string moduleAssemblyPath = await new NuGetClient().InstallModule(programPackageName[..vIndex], version);
		await File.WriteAllTextAsync(primedStatePath + "AssemblyPath", moduleAssemblyPath);
	}
	else
		throw new Exception($"Couldn't parse version string {versionString}");
}
else
{
	List<Task> parallelTasks = new(2);

	// ReSharper disable NotResolvedInText
	string sourcePath = Environment.GetEnvironmentVariable("Dagger:Module:SourcePath")
		?? throw new ArgumentNullException("Dagger:Module:SourcePath");
	bool coreProgram = Environment.GetEnvironmentVariable("Dagger:Module:IsCore") != null;

	string depsPath;
	if (localTest)
		depsPath = $"{sourcePath}/bin/Release/net8.0/linux-x64/Dagger.{sourcePath}.deps.json";
	else if (coreProgram)
		depsPath = $"{sourcePath}/Dagger.{sourcePath.AsSpan()[1..]}.deps.json";
	else
	{
		string moduleName = Environment.GetEnvironmentVariable("Dagger:Module:Name")
			?? throw new ArgumentNullException("Dagger:Module:Name");
		ModuleAssemblyFile moduleAssemblyFile = new(moduleName, sourcePath);
		depsPath = moduleAssemblyFile.File.FullName[..^3] + "deps.json";
		parallelTasks.Add(File.WriteAllTextAsync(primedStatePath + "AssemblyPath", moduleAssemblyFile.File.FullName));
	}
	Console.WriteLine($"Restoring {depsPath}");

	NuGetClient nuGetClient = new(sourcePath);
	// When it comes to user modules, use the default (/Dependencies) path is used.  This works because user modules are
	// loaded by Thunk through an Assembly Load Context that knows to look there.  Core programs are loaded by the
	// dotnet runtime executable, which *does* have a parameter --additionalprobingpath that can be used to give it
	// additional directories (like /Dependencies) to probe for assemblies... but for such paths it expects a NuGet
	// style directory structure (e.g. /Dependencies/graphql.client/6.0.2/lib/netstandard2.0/GraphQL.Client.dll).  To
	// keep everything as simple and consistent as possible, core programs' dependencies are thus placed in the same
	// directory as the core program itself.
	string? installPath = coreProgram ? sourcePath + '/' : null;
	parallelTasks.Add
	(
		nuGetClient.RestoreModule(File.OpenRead(depsPath), depsPath[..(depsPath.LastIndexOf('/') + 1)], installPath)
	);

	if (coreProgram && sourcePath == "/CodeGenerator")
		await nuGetClient.InstallReferenceAssemblies();

	foreach (Task parallelTask in parallelTasks)
		await parallelTask;
}
