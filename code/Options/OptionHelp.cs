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

			ConsoleEx.WriteLog($"ProcDumpEx {(version is not null ? $"v{version}" : string.Empty)} - Extension to Microsoft's Sysinternal Tool ProcDump", logId);
			ConsoleEx.WriteLog($"Copyright (C) Alexander Peipp and Daniel Eichner", logId);
			ConsoleEx.WriteEmptyLine();

			ConsoleEx.WriteUnderline("ProcDumpEx-Help:", logId);
			ConsoleEx.WriteEmptyLine();
			ConsoleEx.WriteLog("ProcDumpEx extends ProcDump with additional functionality, such as process monitoring and /or simplified parameter input.", logId);
			ConsoleEx.WriteLog("For a better overview, https://github.com/PoppyTheDeveloPaw/ProcDumpEx can be visited", logId);
			ConsoleEx.WriteEmptyLine();
			ConsoleEx.WriteUnderline("ProcDumpEx provides the following additional parameters:", logId);
			ConsoleEx.WriteEmptyLine();
			foreach (var type in Helper.GetTypesWithOptionAttribute(Assembly.GetExecutingAssembly()))
			{
				ConsoleEx.WriteUnderline(type.GetOption(), logId);

				foreach (string descLine in type.GetDescription())
				{
					ConsoleEx.WriteLog(descLine, logId);
				}

				ConsoleEx.WriteEmptyLine();
			}
			ConsoleEx.WriteEmptyLine();
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
				ConsoleEx.WriteLog(e.Message, logId, LogType.Error);
			}

			process.Start();
			await process.WaitForExitAsync();
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			throw new NotSupportedException();
		}
	}
}
