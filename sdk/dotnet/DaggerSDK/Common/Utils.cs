namespace Dagger;

static class Utils
{
	public static void Log(string? stack)
	{
		ConsoleColor originalBackgroundColor = Console.BackgroundColor;
		ConsoleColor originalForegroundColor = Console.ForegroundColor;
		Console.BackgroundColor = ConsoleColor.Red;
		Console.ForegroundColor = ConsoleColor.Black;
		Console.WriteLine(stack);
		Console.BackgroundColor = originalBackgroundColor;
		Console.ForegroundColor = originalBackgroundColor;
	}
}
