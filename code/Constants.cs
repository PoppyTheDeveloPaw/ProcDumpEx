namespace ProcDumpEx
{
	internal record Constants
	{
		internal const string RelativeProcdumpPath = @".\Procdump\procdump.exe";
		internal const string RelativeProcdump64Path = @".\Procdump64\procdump64.exe";

		internal static string FullProcdumpPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), RelativeProcdumpPath));
		internal static string FullProcdump64Path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), RelativeProcdump64Path));

		internal static string ProcessPlaceholder = "[ProcessPlaceholder]";
	}
}
