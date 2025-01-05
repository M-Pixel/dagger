using System.Reflection;
using System.Runtime.Loader;

namespace Dagger.Thunk;

class ThunkAssemblyResolver : MetadataAssemblyResolver
{
	private readonly AssemblyDependencyResolver _localResolver;
	private readonly AssemblyDependencyResolver _systemResolver = new(typeof(object).Assembly.Location);


	public ThunkAssemblyResolver(string pathToDependentAssembly)
	{
		_localResolver = new AssemblyDependencyResolver(pathToDependentAssembly);
	}


	public override Assembly? Resolve(MetadataLoadContext context, AssemblyName assemblyName)
	{
		string? path = _localResolver.ResolveAssemblyToPath(assemblyName);
		path ??= _systemResolver.ResolveAssemblyToPath(assemblyName);
		return path == null ? null : context.LoadFromAssemblyPath(path);
	}
}
