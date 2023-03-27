namespace ProcDumpEx
{
	internal record Constants
	{
		internal const string ProcDumpFileName = "procdump.exe";
		internal const string ProcDump64FileName = "procdump64.exe";
		internal const string ProcDump64aFileName = "procdump64a.exe";

		internal const string RelativeProcdumpFolderPath = @$".\Procdump\{ProcDumpFileName}";
		internal const string RelativeProcdumpPath = @$".\{ProcDumpFileName}";

		internal const string RelativeProcdump64FolderPath = @$".\Procdump64\{ProcDump64FileName}";
		internal const string RelativeProcdump64Path = @$".\{ProcDump64FileName}";

		internal const string RelativeProcdump64aFolderPath = @$".\Procdump64a\{ProcDump64aFileName}";
		internal const string RelativeProcdump64aPath = @$".\{ProcDump64aFileName}";

		internal static string FullProcdump64Path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), RelativeProcdump64Path));
		internal static string FullProcdump64FolderPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), RelativeProcdump64FolderPath));

		internal static string FullProcdumpPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), RelativeProcdumpPath));
		internal static string FullProcdumpFolderPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), RelativeProcdumpFolderPath));

		internal static string FullProcdump64aPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), RelativeProcdump64aPath));
		internal static string FullProcdump64aFolderPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), RelativeProcdump64aFolderPath));

		internal static string ProcessPlaceholder = "[ProcessPlaceholder]";
	}
}
