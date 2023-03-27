namespace ProcDumpEx
{
	internal record Constants
	{
		internal const string ProcDumpFileName = "procdump.exe";
		internal const string ProcDump64FileName = "procdump64.exe";

		internal const string RelativeProcdumpFolderPath = @$".\Procdump\{ProcDumpFileName}";
		internal const string RelativeProcdumpPath = @$".\{ProcDumpFileName}";

		internal const string RelativeProcdump64FolderPath = @$".\Procdump64\{ProcDump64FileName}";
		internal const string RelativeProcdump64Path = @$".\{ProcDump64FileName}";

		internal static string FullProcdumpFolderPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), RelativeProcdumpFolderPath));
		internal static string FullProcdump64FolderPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), RelativeProcdump64FolderPath));

		internal static string FullProcdumpPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), RelativeProcdumpPath));
		internal static string FullProcdump64Path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), RelativeProcdump64Path));


		internal static string ProcessPlaceholder = "[ProcessPlaceholder]";
	}
}
