using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Dagger;
using Dagger.Thunk;
using Module = Dagger.Module;
// ReSharper disable NotResolvedInText

if (Environment.GetCommandLineArgs().Contains("-DebugIntrospection"))
{
	FileInfo moduleAssemblyFileInfo = new("Pipelines/bin/Release/net8.0/linux-x64/Dagger.Pipelines.dll");
	MetadataLoadContext metadataLoader = new(new ThunkAssemblyResolver(moduleAssemblyFileInfo.FullName));
	Assembly moduleAssembly = metadataLoader.LoadFromStream(moduleAssemblyFileInfo.OpenRead());
	new Introspection(moduleAssembly, "Pipelines")
		.Build(await ElementDocumentation.Parse(new FileStream("Pipelines/bin/Release/net8.0/linux-x64/Dagger.Pipelines.xml", FileMode.Open)));
	return;
}

// Kick off Dagger query first without awaiting it, so that assembly loading (which doesn't have async methods) can
// happen in parallel.
Query dag = Query.FromDefaultSession;
FunctionCall functionCall = dag.CurrentFunctionCall();
Task<string> parentNameTask = functionCall.ParentName();

string moduleName = Environment.GetEnvironmentVariable("Dagger:Module:Name")
	?? throw new ArgumentNullException("Dagger:Module:Name");
string moduleAssemblyPath = await System.IO.File.ReadAllTextAsync("/etc/dagger/AssemblyPath");

string parentName = await parentNameTask;
if (parentName == "")
{
	// The entrypoint was called for the purpose of introspecting the module, rather than for invoking it.
	string documentationPath = moduleAssemblyPath[..^3] + "xml";
	Task<ElementDocumentation>? documentationTask = System.IO.File.Exists(documentationPath)
		? ElementDocumentation.Parse(new FileStream(documentationPath, FileMode.Open, FileAccess.Read, FileShare.Read))
		: null;

	MetadataLoadContext metadataLoader = new(new ThunkAssemblyResolver(moduleAssemblyPath));
	Assembly moduleAssembly = metadataLoader.LoadFromAssemblyPath(moduleAssemblyPath);
	Introspection introspection = new(moduleAssembly, moduleName);

	Module module = introspection.Build(documentationTask == null ? new() : await documentationTask);
	ModuleID moduleID = await module.Id();
	await functionCall.ReturnValue(new JSON($"\"{moduleID.Value}\""));
}
else
{
	AssemblyLoadContext assemblyLoader = new DaggerModuleLoadContext(moduleAssemblyPath);
	Assembly moduleAssembly = assemblyLoader.LoadFromAssemblyPath(moduleAssemblyPath);
	await new Invocation(moduleAssembly).Run(functionCall, parentName, moduleName);
}
