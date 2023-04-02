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

		internal override async Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			ConsoleEx.WriteUnderline("ProcDumpEx-Help:");
			ConsoleEx.WriteLine();
			ConsoleEx.WriteLine("ProcDumpEx extends ProcDump with additional functionality, such as process monitoring and /or simplified parameter input.");
			ConsoleEx.WriteLine("For a better overview, https://github.com/PoppyTheDeveloPaw/ProcDumpEx can be visited");
			ConsoleEx.WriteLine();
			ConsoleEx.WriteUnderline("ProcDumpEx provides the following additional parameters:");
			ConsoleEx.WriteLine();
			foreach (var type in Helper.GetTypesWithOptionAttribute(Assembly.GetExecutingAssembly()))
			{
				ConsoleEx.WriteUnderline(type.GetOption());
				ConsoleEx.WriteLine(type.GetDescription());
				ConsoleEx.WriteLine();
			}
			ConsoleEx.WriteLine();
			ConsoleEx.WriteUnderline("Below is the usage of procdump itself");
			var process = new Process();

			try
			{
				process.StartInfo = new ProcessStartInfo(Helper.GetExistingProcDumpPath(), "-?")
				{
					UseShellExecute = false
				};
			}
			catch (ProcDumpFileMissingException e)
			{
				ConsoleEx.WriteError(e.Message);
				return false;
			}

			process.Start();
			await process.WaitForExitAsync();
			return true;
		}
	}
}
