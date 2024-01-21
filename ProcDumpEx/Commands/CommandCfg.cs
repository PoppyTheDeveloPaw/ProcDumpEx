using ProcDumpEx.Exceptions;
using ProcDumpEx.Utilities;

namespace ProcDumpEx.Commands;

/// <summary>
/// Represents a command for ProcDumpEx that allows specifying a configuration file containing multiple ProcDumpEx commands.
/// <para>
/// With the prameter -cfg the path to a file can be specified, in which several ProcDumpEx commands stand. Here it is to be paid attention to the fact that per line exactly one ProcDumpEx command may stand.
/// </para>
/// </summary>
internal class CommandCfg : ICommand
{
	/// <summary>
	/// The constant representing the command name.
	/// </summary>
	public const string CommandName = "-cfg";

	/// <inheritdoc />
	public string GetCommandName() => CommandName;

	/// <summary>
	/// Paths to the configuration file(s)
	/// </summary>
	private readonly string[] _cfgPaths;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="CommandCfg"/> class with the specified configuration file path(s).
	/// </summary>
	/// <param name="values">The paths to the configuration file(s).</param>
	public CommandCfg(params string[] values)
	{
		_cfgPaths = values;
	}

	/// <summary>
	/// Gets the arguments from the configuration file(s).
	/// </summary>
	/// <returns>A list of lists, where each inner list represents the arguments for a ProcDumpEx command.</returns>
	internal List<(LineInfo LineInfo, List<string> Args)> GetCfgFileArguments()
	{
		var linesAllFiles = GetAllLines();

		var fileArguments = new List<(LineInfo LineInfo, List<string> Args)>();

		foreach (var args in linesAllFiles.Distinct().Select(o => (o.LineInfo, SplitToArguments(o.LineContent))))
		{
			if (args.Item2 is null)
			{
				// Could not be splitted correctly to arguments.
				continue;
			}
			fileArguments.Add((args.LineInfo, args.Item2));
		}

		return fileArguments;
	}

	/// <summary>
	/// Reads all lines from the specified configuration file(s) and processes them.
	/// </summary>
	/// <param name="log">Defines if an log entry for invalid paths should be added.</param>
	/// <returns>A list of processed lines from the configuration file(s).</returns>
	internal List<(LineInfo LineInfo, string LineContent)> GetAllLines(bool log = true)
	{
		List<(LineInfo LineInfo, string LineContent)> linesOfAllFiles = new();

		foreach (string path in _cfgPaths)
		{
			if (!File.Exists(path) && log)
			{
				Logger.AddOutput($"The specified config file path \"{path}\" is invalid. The path is ignored.", logType: LogType.Error);
				continue;
			}

			var lines = File.ReadAllLines(path).Select(str => Utils.TrimStart(str, "procdumpex.exe ", true)).ToList();

			for (int i = 0; i < lines.Count(); i++)
			{
				if (string.IsNullOrWhiteSpace(lines[i]))
				{
					continue;
				}
				linesOfAllFiles.Add((new LineInfo(path, i + 1), lines[i]));
			}
		}

		return linesOfAllFiles;
	}

	/// <summary>
	/// Splits a string into arguments, considering quoted parts.
	/// </summary>
	/// <param name="arg">The string to be split into arguments.</param>
	/// <returns>A list of arguments obtained from the input string.</returns>
	internal List<string>? SplitToArguments(string arg) 
	{
		List<string> result = new List<string>();

		var splitResult = arg.Split(' ');
		for (int i = 0; i < splitResult.Length; i++)
		{
			var item = splitResult[i];

			if (!item.StartsWith('"'))
			{
				result.Add(item);
				continue;
			}

			try
			{
				result.Add(ExtractQuotedPart(splitResult, ref i));
			}
			catch (EndQuoteMissingException)
			{
				Logger.AddOutput($"A closing quote is missing in the line \"{arg}\" and is therefore ignored.", logType: LogType.Error);
				return null;
			}
		}

		return result;
	}

	/// <summary>
	/// Extracts a quoted part from an array of strings starting from a specified index.
	/// </summary>
	/// <param name="inputArray">The array of strings from which to extract the quoted part.</param>
	/// <param name="currentIndex">The starting index from which to begin the extraction. Updated to the index of the last processed element.</param>
	/// <returns>
	/// A string representing the quoted part extracted from the array. The extracted part includes the leading and trailing double-quote characters.
	/// </returns>
	/// <exception cref="EndQuoteMissingException">
	/// Thrown if the end quote for the quoted part is not found within the provided array.
	/// </exception>
	internal string ExtractQuotedPart(string[] inputArray, ref int currentIndex)
	{
		List<string> quotedParts = new List<string>();

		for (int i = currentIndex; i < inputArray.Length; i++)
		{
			var item = inputArray[i];
			quotedParts.Add(item);
			if (item.EndsWith('"'))
			{
				currentIndex = i;
				return string.Join(' ', quotedParts);
			}
		}

		throw new EndQuoteMissingException();
	}

	/// <inheritdoc />
	public bool Validate(LineInfo? lineInfo)
	{
		if (lineInfo is not null)
		{
			Logger.AddOutput($"{lineInfo}: It is not possible to refer to another cfg file within the cfg file.", logType: LogType.Error);
			return false;
		}

		if (!GetAllLines(false).Any())
		{
			Logger.AddOutput($"If the {CommandName} parameter is used, at least one configuration file path is expected as a parameter.", logType: LogType.Error);
			return false;
		}
		return true;
	}
}
