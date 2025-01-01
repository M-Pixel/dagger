using System.Reflection;
using System.Runtime.Loader;
using Dagger;
using Dagger.Thunk;
using Module = Dagger.Module;

// Kick off Dagger query first without awaiting it, so that assembly loading (which doesn't have async methods) can
// happen in parallel.
Client dag = Client.Default;
FunctionCall functionCall = dag.CurrentFunctionCall();
Task<string> parentNameTask = functionCall.ParentName();

// Bare-bones argument parsing because it's an internal API
string moduleName = Environment.GetCommandLineArgs()[1];
string sourceSubPath = Environment.GetCommandLineArgs()[2];
var assemblyStreams = AssemblyStreamSet.Locate(moduleName, sourceSubPath);
AssemblyLoadContext assemblyLoader = new("Dagger module");
Assembly moduleAssembly = assemblyLoader.LoadFromStream(assemblyStreams.Bytecode, assemblyStreams.Symbols);

// If I don't yet know whether this is a registration call or an invocation call, start reading the documentation, so
// that the overall latency is reduced in case it turns out to be a registration call.
CancellationTokenSource documentationCancellationSource = new();
Task<ElementDocumentation>? documentationTask =
	parentNameTask.IsCompletedSuccessfully || assemblyStreams.Documentation == null
		? null
		: ElementDocumentation.Parse(assemblyStreams.Documentation, documentationCancellationSource.Token);
string parentName = await parentNameTask;
if (parentName == "")
{
	// The entrypoint was called for the purpose of introspecting the module, rather than for invoking it.
	if (documentationTask == null && assemblyStreams.Documentation != null)
		documentationTask = ElementDocumentation.Parse(assemblyStreams.Documentation);

	Introspection introspection = new(moduleAssembly, moduleName);
	Module module = introspection.BuildModule(documentationTask == null ? new() : await documentationTask);
	ModuleID moduleID = await module.Id();
	await functionCall.ReturnValue(new JSON($"\"{moduleID.Value}\""));
}
else
{
	// Invoking a function, don't need documentation. If preemptive documentation reading had started, stop it.
	_ = documentationCancellationSource.CancelAsync();
	await new Invocation(moduleAssembly).Run(functionCall, parentName, moduleName);
}
