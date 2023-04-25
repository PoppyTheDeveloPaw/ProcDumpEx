using ProcDumpEx.Exceptions;

namespace ProcDumpEx
{
	internal static class ArgumentManager
	{
		public static ProcDumpExCommand[]? GetCommands(string[] args)
		{
			if (args.Length == 0)
			{
				ConsoleEx.WriteError($"For the execution of ProcDumpEx parameters are expected. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters", "ArgumentManager");
				return null;
			}

			List<ProcDumpExCommand> commands = new List<ProcDumpExCommand>();
			int idCounter = 1;
			try
			{
				if(ProcDumpExCommandParser.Parse(string.Join(' ', args), 1) is not { } command)
				{
					ConsoleEx.WriteError($"Specified parameters ({string.Join(' ', args)}) could not be parsed. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters", "ArgumentManager");
					return null;
				}
				
				if (command.OptionCfg is null)
					return new[] { command };

				foreach (var argsCommandLine in command.OptionCfg.GetArgumentsFromFile())
				{
					if (argsCommandLine.Length == 0)
					{
						ConsoleEx.WriteError($"For the execution of ProcDumpEx parameters are expected. Empty line in configuration file is ignored. Use the parameter \"-help\" to display examples and allowed parameters", "ArgumentManager");
						continue;
					}

					if (ProcDumpExCommandParser.Parse(argsCommandLine, idCounter++) is not { } cfgCommand)
					{
						ConsoleEx.WriteError($"Specified parameters ({argsCommandLine}) could not be parsed and are therefore ignored. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters", "ArgumentManager");
						continue;
					}

					if (cfgCommand.OptionCfg is not null)
					{
						ConsoleEx.WriteError($"The -cfg parameter could not be used in the configuration file. The parameters ({argsCommandLine}) are ignored", "ArgumentManager");
						continue;
					}

					commands.Add(cfgCommand);
				}
			}
			catch (ManageArgumentsException e)
			{
				ConsoleEx.WriteError(e.Message, "ArgumentManager");
				return null;
			}

			return commands.Any() ? commands.ToArray() : null;
		}
	}
}
