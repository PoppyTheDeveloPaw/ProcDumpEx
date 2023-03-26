﻿using System.Diagnostics;
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
			Console.WriteLine();
			Console.WriteLine("ProcDumpEx extends ProcDump with additional functionality, such as process monitoring and /or simplified parameter input.");
			Console.WriteLine("For a better overview, https://github.com/PoppyTheDeveloPaw/ProcDumpEx can be visited");
			Console.WriteLine();
			ConsoleEx.WriteUnderline("ProcDumpEx provides the following additional parameters:");
			Console.WriteLine();
			foreach (var type in Helper.GetTypesWithOptionAttribute(Assembly.GetExecutingAssembly()))
			{
				ConsoleEx.WriteUnderline(type.GetOption());
				Console.WriteLine(type.GetDescription());
				Console.WriteLine();
			}
			Console.WriteLine();
			ConsoleEx.WriteUnderline("Below is the usage of procdump itself");
			var process = new Process();
			process.StartInfo = new ProcessStartInfo(Constants.FullProcdumpPath, "-?")
			{
				UseShellExecute = false
			};

			process.Start();
			await process.WaitForExitAsync();
			return true;
		}
	}
}