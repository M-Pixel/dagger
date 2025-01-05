using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Dagger.Generated.ModuleTest;
using Dagger.Thunk;
using Module = Dagger.Generated.ModuleTest.Module;

if (Environment.GetCommandLineArgs().Contains("-DebugIntrospection"))
{
	FileInfo moduleAssemblyFileInfo = new("ModuleTest/bin/Release/net8.0/linux-x64/ModuleTest.dll");
	MetadataLoadContext metadataLoader = new(new ThunkAssemblyResolver(moduleAssemblyFileInfo.FullName));
	Assembly moduleAssembly = metadataLoader.LoadFromStream(moduleAssemblyFileInfo.OpenRead());
	new Introspection(moduleAssembly, "ModuleTest").Build(new ElementDocumentation());
	return;
}

// Kick off Dagger query first without awaiting it, so that assembly loading (which doesn't have async methods) can
// happen in parallel.
Query dag = Query.FromDefaultSession;
FunctionCall functionCall = dag.CurrentFunctionCall();
Task<string> parentNameTask = functionCall.ParentName();

// Bare-bones argument parsing because it's an internal API
string moduleName = Environment.GetCommandLineArgs()[1];
string sourceSubPath = Environment.GetCommandLineArgs()[2];
ModuleAssemblyFile moduleAssemblyFile = new(moduleName, sourceSubPath);

string parentName = await parentNameTask;
if (parentName == "")
{
	// The entrypoint was called for the purpose of introspecting the module, rather than for invoking it.
	Stream? documentationStream = moduleAssemblyFile.Documentation();
	var documentationTask = documentationStream == null ? null : ElementDocumentation.Parse(documentationStream);

	MetadataLoadContext metadataLoader = new(new ThunkAssemblyResolver(moduleAssemblyFile.File.FullName));
	Assembly moduleAssembly = metadataLoader.LoadFromStream(moduleAssemblyFile.File.OpenRead());
	Introspection introspection = new(moduleAssembly, moduleName);

	Module module = introspection.Build(documentationTask == null ? new() : await documentationTask);
	ModuleID moduleID = await module.Id();
	await functionCall.ReturnValue(new JSON($"\"{moduleID.Value}\""));
}
else
{
	AssemblyLoadContext assemblyLoader = new("Dagger module");
	Assembly moduleAssembly =
		assemblyLoader.LoadFromStream(moduleAssemblyFile.File.OpenRead(), moduleAssemblyFile.Symbols());
	await new Invocation(moduleAssembly).Run(functionCall, parentName, moduleName);
}
