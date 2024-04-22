namespace ProcDumpEx.Exceptions
{
	public class ProcDumpFileMissingException(string procDummpName, params string[] paths) : Exception($"{procDummpName} is missing. Expected under: {string.Join(" or ", paths)}")
	{
		internal string ProcDumpName => procDummpName;
		internal List<string> Paths => [.. paths];
	}
}
