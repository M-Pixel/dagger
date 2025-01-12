using System.Collections.Immutable;
using System.IO;
using System.Reflection;

namespace Dagger.Thunk;

class ThunkAssemblyResolver : MetadataAssemblyResolver
{
	private readonly ImmutableArray<string> _searchPaths;


	public ThunkAssemblyResolver(string pathToDependentAssembly)
	{
		_searchPaths =
		[
			Path.GetDirectoryName(pathToDependentAssembly)! + '/',
			"/module-deps/",
			Path.GetDirectoryName(typeof(object).Assembly.Location)! + '/'
		];
	}


	public override Assembly? Resolve(MetadataLoadContext context, AssemblyName assemblyName)
	{
		foreach (var path in _searchPaths)
		{
			string assemblyPath = path + assemblyName.Name + ".dll";
			if (File.Exists(assemblyPath))
				return context.LoadFromAssemblyPath(assemblyPath);
		}
		return null;
	}
}
