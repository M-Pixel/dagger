using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Dagger.Thunk;

/// <summary>
///		I can't use the default load context to load the module assembly, because the default context already has a
///		version of Dagger.Generated loaded into it that may not contain the module-dependency generated members that the
///		module expects.
/// </summary>
class DaggerModuleLoadContext : AssemblyLoadContext
{
	private readonly AssemblyDependencyResolver _localResolver;

	private readonly AssemblyLoadContext _thunkContext = GetLoadContext(typeof(Session).Assembly)!;

	private readonly ImmutableArray<string> _sharedAssemblyNames =
		["Dagger.Client", ..typeof(Session).Assembly.GetReferencedAssemblies().Select(aName => aName.Name)];


	public DaggerModuleLoadContext(string pathToModuleAssembly)
		: base("Dagger module")
	{
		_localResolver = new AssemblyDependencyResolver(pathToModuleAssembly);
	}


	protected override Assembly? Load(AssemblyName assemblyName)
	{
		// If module included Dagger.Client in its build folder, or any of Dagger.Client's dependencies, avoid isolating
		// a second instance of that assembly in this load context (link the module against the same instance that the
		// thunk is linked against), as a JsonConverter linked to Thunk's instances of those libraries needs to
		// recognize and instantiate some of their types from/into module-defined classes.
		string simpleAssemblyName = assemblyName.Name!;
		if (_sharedAssemblyNames.Contains(simpleAssemblyName))
			return _thunkContext.LoadFromAssemblyName(new AssemblyName(simpleAssemblyName));

		// The default load context won't look for assemblies in the module's build folder - do so here.
		string? path = _localResolver.ResolveAssemblyToPath(assemblyName);
		// If it's a system or NuGet dependency, just let the global context load it.
		return path == null ? null : LoadFromAssemblyPath(path);
	}

	protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
	{
		string? path = _localResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
		return path == null ? 0 : LoadUnmanagedDllFromPath(path);
	}
}
