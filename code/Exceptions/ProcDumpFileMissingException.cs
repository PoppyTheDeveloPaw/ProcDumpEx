namespace ProcDumpEx.Exceptions
{
	internal class ProcDumpFileMissingException : Exception
	{
		internal string ProcDumpName { get; }
		internal List<string> Paths { get; }

		public ProcDumpFileMissingException(string procDummpName, params string[] paths) : base($"{procDummpName} is missing. Expected under: {string.Join(" or ", paths)}")
		{
			ProcDumpName = procDummpName;
			Paths = paths.ToList();
		}
	}
}
