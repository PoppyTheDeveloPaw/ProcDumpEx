using ProcDumpEx.Exceptions;

namespace ProcDumpEx.code
{
	internal static class ArgumentManager
	{
		private static string[] ManageArguments(string[] args)
		{
			string cfgPath = string.Empty;

			for (int i = 0; i < args.Length; i++)
			{
				string item = args[i];
				if (item == "-cfg")
				{
					if (i + 1 < args.Length)
					{
						cfgPath = args[i + 1];
						break;
					}
					else
					{
						throw new ManageArgumentsException("The -cfg parameter was specified, but there is no config path");
					}
				}
			}

			if (!string.IsNullOrEmpty(cfgPath))
			{
				if (!File.Exists(cfgPath))
					throw new ManageArgumentsException("The specified config path is invalid");
				return File.ReadAllLines(cfgPath);
			}

			return new[] { string.Join(' ', args) };
		}

		public static ProcDumpExCommand[]? GetCommands(string[] args)
		{
			bool succeeded = true;
			List<ProcDumpExCommand> commands = new List<ProcDumpExCommand>();
			try
			{
				foreach (var argsCommandLine in ManageArguments(args))
				{
					if (ProcDumpExCommandParser.Parse(argsCommandLine) is not { } command)
					{
						ConsoleEx.WriteError($"Specified parameters ({argsCommandLine}) could not be parsed. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters");
						succeeded = false;
						continue;
					}
					commands.Add(command);
				}
			}
			catch (ManageArgumentsException e)
			{
				ConsoleEx.WriteError(e.Message);
				succeeded = false;
			}

			return succeeded ? commands.ToArray() : null;
		}
	}
}
