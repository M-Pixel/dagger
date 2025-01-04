using Dagger.Generated.ModuleTest;

namespace ModuleTest;

public static class HelloWorld
{
	public static string Foo()
	{
		return Query.FromDefaultSession.GetHello().SubHello("Salutations", giant: true).Result;
	}
}
