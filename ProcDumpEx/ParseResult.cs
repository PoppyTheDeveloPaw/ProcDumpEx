using ProcDumpEx.Commands;
using ProcDumpEx.Exceptions;
using ProcDumpEx.Utilities;
using System.Text.RegularExpressions;

namespace ProcDumpEx;

internal class ParseResult
{
	/// <summary>
	/// Arguments that are used when executing ProcDump
	/// </summary>
	internal readonly string BaseProcdumpCommand;

	/// <summary>
	/// Arguments that are not part of ProcDump, but are added by this extension
	/// </summary>
	internal readonly IReadOnlyList<ICommand> Commands;


	internal readonly (IReadOnlyList<string> ProcessNames, IReadOnlyList<int> ProcessIds) Processes;

	public ParseResult(List<Argument> arguments, bool argumentsFromCfgFile)
	{
		//Check if parameter exists more than once
		foreach (var group in arguments.GroupBy(o => o.Command))
		{
			if (group.Count() > 1)
			{
				if (argumentsFromCfgFile)
				{
					Logger.AddOutput($"The \"{group.Key}\" command has been defined more than once, the line in the cfg file is ignored. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
				}
				else
				{
					Logger.AddOutput($"The \"{group.Key}\" command has been defined more than once. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters.");
				}
				throw new ParseException();
			}
		}

		var commands = ExtractCommands(arguments, argumentsFromCfgFile);

		if (commands.Any(o => o is CommandCfg))
		{
			if (argumentsFromCfgFile)
			{
				Logger.AddOutput("It is not possible to refer to another cfg file within the cfg file. The line is ignored. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
				throw new ParseException();
			}
			else
			{
				Commands = commands;
				Processes = (new List<string>(), new List<int>());
				BaseProcdumpCommand = "";
				return;
			}
		}

        Processes = ExtractProcesses(commands, arguments, argumentsFromCfgFile);
		BaseProcdumpCommand = CreateBaseProcdumpCommands(arguments);
		Commands = commands;
	}

	private string CreateBaseProcdumpCommands(List<Argument> arguments)
	{
		List<string> baseProcdumpCommand = new List<string>();

		foreach (var arg in arguments)
		{
			baseProcdumpCommand.Add(arg.Command);
			if (arg.Arguments is not null && arg.Arguments.Any())
			{
				baseProcdumpCommand.Add($"\"{string.Join(',', arg.Arguments)}\"");
			}
		}

		return string.Join(' ', baseProcdumpCommand);
	}

	private List<ICommand> ExtractCommands(List<Argument> arguments, bool argumentsFromCfgFile)
	{
		List<ICommand> commands = new List<ICommand>();

		for (int i = 0; i < arguments.Count; i++)
		{
			var argument = arguments[i];

			if (!CommandDict.CommandTypes.ContainsKey(argument.Command))
			{
				continue;
			}

			bool parsingFailed = false;
			try
			{
				if (argument.Arguments is null || !argument.Arguments.Any())
				{
					var obj = CommandDict.CommandTypes.CreateObject(argument.Command);
					if (obj is null)
					{
						parsingFailed = true;
					}
					else
					{
						commands.Add(obj);
					}
				}
				else
				{
					var obj = CommandDict.CommandTypes.CreateObject(argument.Command, argument.Arguments.ToArray());
					if (obj is null)
					{
						parsingFailed = true;
					}
					else
					{
						commands.Add(obj);
					}
				}
			}
			catch (MissingMethodException)
			{
				parsingFailed = true;

			}

			if (parsingFailed)
			{
				if (argument.Arguments is null || !argument.Arguments.Any())
				{
					if (argumentsFromCfgFile)
					{
						Logger.AddOutput($"The \"{argument.Command}\" command expects parameters, but these have not been defined. The incorrectly defined line from the cfg file is ignored. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
					}
					else
					{
						Logger.AddOutput($"The \"{argument.Command}\" command expects parameters, but these have not been defined. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
					}
				}
				else
				{
					if (argumentsFromCfgFile)
					{
						Logger.AddOutput($"The \"{argument.Command}\" command does not expect any parameters, but some have been defined. The incorrectly defined line from the cfg file is ignored. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
					}
					else
					{
						Logger.AddOutput($"The \"{argument.Command}\" command does not expect any parameters, but some have been defined. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
					}
				}

				throw new ParseException();
			}

			arguments.RemoveAt(i);
			i--;
		}

		return commands;
	} 

	private (IReadOnlyList<string> ProcessNames, IReadOnlyList<int> ProcessIds) ExtractProcesses(List<ICommand> commands, List<Argument> arguments, bool argumentsFromCfgFile)
	{
		var commandPn = commands.FirstOrDefault(o => o is CommandPn) as CommandPn;
		commands.RemoveAll(o => o is CommandPn);

		IReadOnlyList<string>? lastArgProcesses = null;
		if (arguments.Last().Command == string.Empty)
		{
			lastArgProcesses = arguments.Last().Arguments;
			arguments.RemoveAt(arguments.Count - 1);
		}

		List<string> processes = new List<string>();
		
		if (commandPn is not null)
		{
			processes.AddRange(commandPn.Processes);
		}

		if (lastArgProcesses is not null)
		{
			processes.AddRange(lastArgProcesses);
		}

		if (!processes.Any())
		{
			if (argumentsFromCfgFile)
			{
				Logger.AddOutput("No process name or process id was specified, the line in the Cfg file is ignored. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
			}
			else
			{
				Logger.AddOutput("No process name or process id was specified. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
			}

			throw new ParseException();
		}

		List<string> processNames = new();
		List<int> processIds = new();

		foreach (var processParam in processes)
		{
			if (int.TryParse(processParam, out int id))
			{
				processIds.Add(id);
				continue;
			}

			if (Regex.IsMatch(processParam, @"[.]*\.exe$"))
			{
				processNames.Add(processParam);
				continue;
			}

			if (argumentsFromCfgFile)
			{
				Logger.AddOutput($"No valid process name or process id: {processParam}. Only process IDs (numeric values) or process names (*.exe) are expected. The line in the Cfg file is ignored. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
			}
			else
			{
				Logger.AddOutput($"No valid process name or process id: {processParam}. Only process IDs (numeric values) or process names (*.exe) are expected. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
			}
			throw new ParseException();
		}

		if (!processIds.Any() && !processNames.Any())
		{
			if (argumentsFromCfgFile)
			{
				Logger.AddOutput("No process name or process id was specified, the line in the Cfg file is ignored. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
			}
			else
			{
				Logger.AddOutput("No process name or process id was specified. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
			}

			throw new ParseException();
		}

		return (processNames.Distinct().ToList(), processIds.Distinct().ToList());
	}
}
