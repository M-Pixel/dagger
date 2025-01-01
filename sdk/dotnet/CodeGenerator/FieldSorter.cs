namespace Dagger.Generator;

class FieldSorter : IComparer<string?>
{
	public static readonly FieldSorter instance = new();

	private FieldSorter() {}

	public int Compare(string? x, string? y)
	{
		if (x == "id" && y == "id")
			return 1;
		if (x == "id")
			return -1;
		if (y == "id")
			return 1;
		return String.CompareOrdinal(x, y);
	}
}
