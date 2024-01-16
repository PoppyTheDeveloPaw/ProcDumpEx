namespace ProcDumpEx.Utilities;

internal class Constants
{
	internal static string[] AbsolutePath_Procdump64 =
		[
			Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @".\procdump64.exe")),
			Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @".\Procdump64\procdump64.exe"))
		];

	internal static string[] AbsolutePath_Procump = 
		[
			Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @".\procdump.exe")),
			Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @$".\Procdump\procdump.exe"))
		];

	internal static string[] AbsolutePath_Procdump64a = 
		[
			Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @".\procdump64a.exe")),
			Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @".\Procdump64a\procdump64a.exe"))
		];

	internal static string ProcessPlaceholder = "[ProcessPlaceholder]";
}
