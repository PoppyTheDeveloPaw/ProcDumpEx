using ProcDumpEx.Exceptions;
using System.Diagnostics;
using System.Reflection;

namespace ProcDumpEx.Options
{
	[Option("-help", false)]
	public class OptionHelp : OptionBase
	{
		internal override bool IsCommandCreator => false;

		public OptionHelp()
		{
		}

		internal static async Task WriteHelp(string logId)
		{
			Version? version = Assembly.GetExecutingAssembly().GetName().Version;

			ConsoleEx.WriteLine($"ProcDumpEx {(version is not null ? $"v{version}" : string.Empty)} - Extension to Microsoft's Sysinternal Tool ProcDump", logId);
			ConsoleEx.WriteLine($"Copyright (C) Alexander Peipp and Daniel Eichner", logId);
			ConsoleEx.WriteLine();

			ConsoleEx.WriteUnderline("ProcDumpEx-Help:", logId);
			ConsoleEx.WriteLine();
			ConsoleEx.WriteLine("ProcDumpEx extends ProcDump with additional functionality, such as process monitoring and /or simplified parameter input.", logId);
			ConsoleEx.WriteLine("For a better overview, https://github.com/PoppyTheDeveloPaw/ProcDumpEx can be visited", logId);
			ConsoleEx.WriteLine();
			ConsoleEx.WriteUnderline("ProcDumpEx provides the following additional parameters:", logId);
			ConsoleEx.WriteLine();
			foreach (var type in Helper.GetTypesWithOptionAttribute(Assembly.GetExecutingAssembly()))
			{
				ConsoleEx.WriteUnderline(type.GetOption(), logId);

				foreach (string descLine in type.GetDescription())
					ConsoleEx.WriteLine(descLine, logId);

				ConsoleEx.WriteLine();
			}
			ConsoleEx.WriteLine();
			ConsoleEx.WriteUnderline("Below is the usage of procdump itself", logId);
			var process = new Process();

			try
			{
				process.StartInfo = new ProcessStartInfo(Helper.GetExistingProcDumpPath(ProcDumpVersion.ProcDump), "-?")
				{
					UseShellExecute = false
				};
			}
			catch (ProcDumpFileMissingException e)
			{
				ConsoleEx.WriteError(e.Message, logId);
			}

			process.Start();
			await process.WaitForExitAsync();
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			throw new NotImplementedException();
		}
	}
}
