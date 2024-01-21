using ProcDumpEx.Commands;
using ProcDumpEx.Exceptions;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace ProcDumpEx.Utilities;

internal class ArgumentParser
{
	private string[] _arguments;

	/// <summary>
	/// Creates an instance of <see cref="ArgumentParser"/>
	/// </summary>
	/// <param name="args">Arguments that were specified when the program was started.</param>
	public ArgumentParser(string[] args)
	{
		_arguments = args.ToArray();
		FixArguments();
	}

	/// <summary>
	/// Brings the arguments into a specific format
	/// </summary>
	private void FixArguments()
	{
		for (int i = 0; i < _arguments.Length; i++)
		{
			if (_arguments[i].Contains(','))
			{
				if (!_arguments[i].StartsWith('"'))
					_arguments[i] = $"\"{_arguments[i]}";
				if (!_arguments[i].EndsWith('"'))
					_arguments[i] = $"{_arguments[i]}\"";
			}
		}

		for (int i = 0; i < _arguments.Length; i++)
		{
			if (_arguments[i].Contains(' '))
			{
				if (!_arguments[i].StartsWith('"'))
					_arguments[i] = $"\"{_arguments[i]}";
				if (!_arguments[i].EndsWith('"'))
					_arguments[i] = $"{_arguments[i]}\"";
			}
		}
	}

	/// <summary>
	/// Converts the passed arguments into command and parameter pairs.
	/// </summary>
	/// <returns>A list of command/parameter pairs.</returns>
	/// <exception cref="ParseException">Is thrown if the specified arguments could not be converted.</exception>
	private ParseResult Parse(bool lineFromConfigFile)
	{
		var arguments = GetArguments();
		CheckForNoDuplicates(arguments, lineFromConfigFile);
		var commands = ExtractCommands(arguments, lineFromConfigFile);

		CheckCommandValidity(commands, lineFromConfigFile);

		if (commands.Any(o => o is CommandCfg))
		{
			return new ParseResult(commands, default, "");
		}

		// Removes -pn commands
		var processes = ExtractProcesses(commands, arguments, lineFromConfigFile);
		var baseProcdumpParameter = CreateBaseProcdumpCommands(arguments);

		return new ParseResult(commands, processes, baseProcdumpParameter);
	}

	/// <summary>
	/// Extracts individual arguments from the provided array of command-line arguments.
	/// </summary>
	/// <returns>A list of <see cref="Argument"/> objects representing parsed command-line arguments.</returns>
	private List<Argument> GetArguments()
	{
		List<Argument> arguments = new List<Argument>();

		for (int i = 0; i < _arguments.Length; i++)
		{
			var argument = _arguments[i];

			if (!argument.StartsWith('-'))
			{
				var args = ParseCommandArguments(argument);
				if (args.Any())
				{
					arguments.Add(new Argument(string.Empty, args));
				}
				else
				{
					//Should never happen
					throw new ParseException();
				}

				continue;
			}

			if (i + 1 < _arguments.Length && !_arguments[i + 1].StartsWith("-"))
			{
				arguments.Add(new Argument(argument, ParseCommandArguments(_arguments[i + 1])));
				i++;
			}
			else
			{
				arguments.Add(new Argument(argument));
			}
		}

		return arguments;
	}

	/// <summary>
	/// Splits the passed string at commas and removes spaces and quotation marks at the start and end of the string.
	/// </summary>
	/// <param name="args">A string containing command arguments separated by commas.</param>
	/// <returns>A list of command arguments</returns>
	private List<string> ParseCommandArguments(string args)
	{
		List<string> commandArguments = new List<string>();
		foreach (var arg in args.Split(','))
		{
			commandArguments.Add(arg.TrimStart(' ', '"').TrimEnd(' ', '"'));
		}
		return commandArguments;
	}

	/// <summary>
	/// Checks the validity of commands and their parameters.
	/// </summary>
	/// <param name="commands">List of commands to be validated.</param>
	/// <param name="argumentsFromCfgFile">Flag indicating whether the arguments are from a configuration file.</param>
	private void CheckCommandValidity(IReadOnlyList<ICommand> commands, bool argumentsFromCfgFile)
	{
		bool commandsAreValid = true;
		foreach (var command in commands)
		{
			commandsAreValid &= command.Validate();
		}

		if (commands.Any(o => o is CommandCfg) && argumentsFromCfgFile)
		{
			commandsAreValid = false;
			Logger.AddOutput("It is not possible to refer to another cfg file within the cfg file.", logType: LogType.Error);
		}

		if (!commandsAreValid)
		{
			if (argumentsFromCfgFile)
			{
				Logger.AddOutput("As the specified parameters are invalid, the line is ignored. The line is ignored. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
			}
			else
			{
				Logger.AddOutput("As the specified parameters are invalid, ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
			}
			throw new ParseException();
		}
	}

	/// <summary>
	/// Checks for duplicate commands.
	/// </summary>
	/// <param name="arguments">List of arguments to be checked for duplicates.</param>
	/// <param name="argumentsFromCfgFile">Flag indicating whether the arguments are from a configuration file.</param>
	private void CheckForNoDuplicates(List<Argument> arguments, bool argumentsFromCfgFile)
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
	}

	/// <summary>
	/// Creates the associated commands from the arguments
	/// </summary>
	/// <param name="arguments">List of parsed arguments.</param>
	/// <param name="argumentsFromCfgFile">Flag indicating whether the arguments are from a configuration file.</param>
	/// <returns>List of <see cref="ICommand"/> objects representing the parsed commands.</returns>
	/// <exception cref="ParseException">Is thrown if the creation of the command from the passed arguments has failed.</exception>
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

	/// <summary>
	/// Extracts process names and IDs from the parsed commands and arguments.
	/// </summary>
	/// <param name="commands">List of parsed commands.</param>
	/// <param name="arguments">List of parsed arguments.</param>
	/// <param name="argumentsFromCfgFile">Flag indicating whether the arguments are from a configuration file.</param>
	/// <returns>A tuple containing lists of process names and process IDs.</returns>
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

	/// <summary>
	/// Creates the base procdump command from the parsed arguments.
	/// </summary>
	/// <param name="arguments">List of parsed arguments.</param>
	/// <returns>A string representing the base procdump command.</returns>
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

	/// <summary>
	/// Gets the parsing result based on the provided arguments.
	/// </summary>
	/// <returns>A list of <see cref="ParseResult"/> objects representing the parsed results.</returns>
	public IReadOnlyList<ParseResult>? GetParsingResult()
	{
		ParseResult? result;
		try
		{
			result = Parse(false);
		}
		catch (ParseException)
		{
			Logger.AddOutput("The specified parameters could not be parsed. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
			return null;
		}

		List<ParseResult> results = new List<ParseResult>();

		if (result.Commands.FirstOrDefault(o => o is CommandCfg) is not CommandCfg commandCfg)
		{
			results.Add(result);
		}
		else
		{
			Logger.AddOutput("The argument \"-cfg\" was found in your input, the commands from the specified file are used for the further execution of the program.", logType: LogType.Info);

			foreach (var line in commandCfg.GetCfgFileArguments())
			{
				try
				{
					ArgumentParser parser = new ArgumentParser(line.ToArray());
					results.Add(parser.Parse(true));
				}
				catch (ParseException)
				{
					continue;
				}
			}
		}

		return results;
	}
}
