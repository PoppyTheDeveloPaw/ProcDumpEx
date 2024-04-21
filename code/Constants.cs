namespace ProcDumpEx
{
	internal record Constants
	{
		private const string _procDumpFileName = "procdump.exe";
		private const string _procDumpFolderName = "Procdump";

		private const string _procDump64FileName = "procdump64.exe";
		private const string _procDump64FolderName = "Procdump64";

		private const string _procDump64aFileName = "procdump64a.exe";
		private const string _procDump64aFolderName = "Procdump64a";

		internal static string ProcessPlaceholder = "[ProcessPlaceholder]";

		internal static IReadOnlyDictionary<ProcDumpVersion, (string FileName, string FolderName)> ProcDumpDict = new Dictionary<ProcDumpVersion, (string FileName, string FolderName)>()
		{
			{ ProcDumpVersion.ProcDump, (_procDumpFileName, _procDumpFolderName) },
			{ ProcDumpVersion.ProcDump64, (_procDump64FileName, _procDump64FolderName) },
			{ ProcDumpVersion.ProcDump64a, (_procDump64aFileName, _procDump64aFolderName) },
		};
	}

	enum ProcDumpVersion
	{
		ProcDump,
		ProcDump64,
		ProcDump64a
	}
}
