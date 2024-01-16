using ProcDumpEx.Commands;
using ProcDumpEx.Exceptions;
using System.Text;

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
		List<string> ParseCommandArguments(string args)
		{
			List<string> commandArguments = new List<string>();
			foreach (var arg in args.Split(',')) 
			{
				commandArguments.Add(arg.TrimStart(' ', '"').TrimEnd(' ', '"'));
			}
			return commandArguments;
		}

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

		return new ParseResult(arguments, lineFromConfigFile);
	}

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
