using ProcDumpEx.Exceptions;

namespace ProcDumpEx.Options
{
	[Option("-cfg", true)]
	internal class OptionCfg : OptionBase
	{
		internal readonly string FilePath;
		internal override bool IsCommandCreator => false;

		public OptionCfg(string path)
		{
			if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
			{
				throw new ArgumentException($"{GetType().GetOption()} expects a path of an existing ProcDumpEx configuration file.");
			}
			FilePath = path;
		}

		internal string[] GetArgumentsFromFile()
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

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			throw new NotImplementedException();
		}
	}
}
