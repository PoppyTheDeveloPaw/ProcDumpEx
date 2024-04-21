using ProcDumpEx.Exceptions;

namespace ProcDumpEx
{
	internal static class ArgumentManager
	{
		public static ProcDumpExCommand[]? GetCommands(string[] args)
		{
			if (args.Length == 0)
			{
				ConsoleEx.WriteLog($"For the execution of ProcDumpEx parameters are expected. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters", "ArgumentManager", LogType.Error);
				return null;
			}

			List<ProcDumpExCommand> commands = [];
			int idCounter = 1;
			try
			{
				if(ProcDumpExCommandParser.Parse(string.Join(' ', args), 1) is not { } command)
				{
					ConsoleEx.WriteLog($"Specified parameters ({string.Join(' ', args)}) could not be parsed. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters", "ArgumentManager", LogType.Error);
					return null;
				}
				
				if (command.OptionCfg is null)
				{
					return [command];
				}

				foreach (var argsCommandLine in command.OptionCfg.GetArgumentsFromFile())
				{
					if (argsCommandLine.Length == 0)
					{
						ConsoleEx.WriteLog($"For the execution of ProcDumpEx parameters are expected. Empty line in configuration file is ignored. Use the parameter \"-help\" to display examples and allowed parameters", "ArgumentManager", LogType.Error);
						continue;
					}

					if (ProcDumpExCommandParser.Parse(argsCommandLine, idCounter++) is not { } cfgCommand)
					{
						ConsoleEx.WriteLog($"Specified parameters ({argsCommandLine}) could not be parsed and are therefore ignored. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters", "ArgumentManager", LogType.Error);
						continue;
					}

					if (cfgCommand.OptionCfg is not null)
					{
						ConsoleEx.WriteLog($"The -cfg parameter could not be used in the configuration file. The parameters ({argsCommandLine}) are ignored", "ArgumentManager", LogType.Error);
						continue;
					}

					commands.Add(cfgCommand);
				}
			}
			catch (ManageArgumentsException e)
			{
				ConsoleEx.WriteLog(e.Message, "ArgumentManager", LogType.Error);
				return null;
			}

			return commands.Count > 0 ? [.. commands] : null;
		}
	}
}
