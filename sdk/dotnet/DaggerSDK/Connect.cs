namespace Dagger;

public delegate Task CallbackFunction(Client client);

public static class Dagger
{
	/// <summary>Executes the given function using the default global Dagger client.</summary>
	/// <example>
	/// <code>
	/// await Connection.Execute(
	/// 	async () =>
	/// 	{
	/// 		await Client.Default
	/// 			.Container()
	/// 			.From("alpine")
	/// 			.WithExec(["apk", "add", "curl"])
	/// 			.WithExec(["curl", "https://dagger.io/"])
	/// 			.Sync()
	/// 	},
	/// 	new ConnectionOptions(LogOutput: Console.OpenStandardError())
	/// );
	/// </code>
	/// </example>
	public static async Task Connection
	(
		Func<Task> function,
		ConnectionOptions? options = null
	)
	{
		await Context.Default.Connection(options ?? new ConnectionOptions());

		try
		{
			await function();
		}
		finally
		{
			Close();
		}
	}

	/// <summary>Closes the global client connection.</summary>
	public static void Close() => Context.Default.Close();

	/// <summary>
	/// Connects to the GraphQL server and initializes a GraphQL client to execute queries on it through the callback.
	/// </summary>
	public static async Task Connect
	(
		CallbackFunction callback,
		ConnectionOptions? options = null
	)
	{
		Context context = new();
		Client client = new(){ Context = context };

		// Initialize connection
		await context.Connection(options ?? new ConnectionOptions());

		// Throw error if versions incompatible
		try
		{
			await client.CheckVersionCompatibility(CLI.VERSION);
		}
		catch (Exception exception)
		{
			Console.Error.WriteLine($"Failed to check version compatibility: {exception}");
		}
		try
		{
			await callback(client);
		}
		finally
		{
			context.Close();
		}
	}
}
