using ProcDumpEx.Utilities;
using System.Text;

namespace ProcDumpEx.Commands;

internal class CommandCfg : ICommand
{
	public const string CommandName = "-cfg";

	public string GetCommandName() => CommandName;

	public bool IsRuntimeOption() => true;

	private readonly string[] _cfgPaths;
	
	public CommandCfg(params string[] values)
	{
		_cfgPaths = values;
	}

	internal List<List<string>> GetCfgFileArguments()
	{
		List<string> linesAllFiles = new List<string>();

		foreach (string path in _cfgPaths)
		{
			if (!File.Exists(path))
			{
				Logger.AddOutput($"The specified config file path \"{path}\" is invalid. The path is ignored.", logType: LogType.Error);
				continue;
			}

			var lines = File.ReadAllLines(path);

			string toRemoveAtStart = "procdumpex.exe ";

			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].StartsWith(toRemoveAtStart, StringComparison.OrdinalIgnoreCase))
				{
					lines[i] = lines[i].Substring(toRemoveAtStart.Length);
				}
			}

			linesAllFiles.AddRange(lines.Where(o => !string.IsNullOrWhiteSpace(o)));
		}

		return linesAllFiles.Distinct().Select(o => SplitToArguments(o)).ToList();
	}

	internal List<string> SplitToArguments(string arg) 
	{
		List<string> result = new List<string>();

		var splitResult = arg.Split(' ');
		for (int i = 0; i < splitResult.Count(); i++)
		{
			var item = splitResult[i];

			if (!item.StartsWith('"'))
			{
				result.Add(item);
				continue;
			}

			List<string> part = new List<string>();

			for (int y = i; y < splitResult.Count(); y++)
			{
				var item2 = splitResult[y];
				part.Add(item2);
				if (item2.EndsWith('"'))
				{
					result.Add(string.Join(' ', part));
					i = y;
					break;
				}
			}
		}

		return result;
	}
}
