using ProcDumpEx.Exceptions;
using ProcDumpEx.Options;

namespace ProcDumpEx.Options
{
	[Option("-cfg", true)]
	internal class OptionCfg(string path) : OptionBase
	{
		internal string FilePath => path;
		internal override bool IsCommandCreator => false;

		internal string[] GetArgumentsFromFile()
		{
			if (!string.IsNullOrEmpty(FilePath))
			{
				if (!File.Exists(FilePath))
					throw new ManageArgumentsException("The specified config path is invalid");

				var lines = File.ReadAllLines(FilePath);

				string toRemoveAtStart = "procdumpex.exe ";

				for (int i = 0; i < lines.Length; i++)
				{
					if (lines[i].StartsWith(toRemoveAtStart, StringComparison.OrdinalIgnoreCase))
					{
						lines[i] = lines[i][toRemoveAtStart.Length..];
					}
				}

				//ignore comments
				return lines.Where(x => !x.StartsWith("//") && !x.StartsWith('#')).ToArray();
			}
			return [];
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			throw new NotImplementedException();
		}
	}
}
