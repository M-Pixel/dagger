using static Dagger.Generated.ModuleTest.Alias;

namespace ModuleTest;

public static class HelloWorld
{
	public static string Foo()
	{
		return DAG.GetHello().SubHello("Salutations", giant: true).Result;
	}
}

public record RecordTest(int Foo);
