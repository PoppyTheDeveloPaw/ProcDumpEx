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

		internal IEnumerable<(int Index, string Content)> GetArgumentsFromFile()
		{
			if (!File.Exists(FilePath))
			{ 
				throw new ManageArgumentsException("The specified config path is invalid"); 
			}

			List<(int Index, string Content)> linesOfInterest = [];
			var content = File.ReadAllLines(FilePath);
			for (int i = 0; i < content.Length; i++)
			{
				linesOfInterest.Add((i + 1, content[i]));
			}
			// Filter out all commented out lines. First char is # or //
			linesOfInterest = linesOfInterest.Where(o => !o.Content.StartsWith('#') && !o.Content.StartsWith("//")).ToList();

			// Remove procdumex.exe at the begin of the line
			string toRemoveAtStart = "procdumpex.exe ";

			for (int i = 0; i < linesOfInterest.Count(); i++)
			{
				if (linesOfInterest[i].Content.StartsWith(toRemoveAtStart, StringComparison.OrdinalIgnoreCase))
				{
					linesOfInterest[i] = (linesOfInterest[i].Index, linesOfInterest[i].Content[toRemoveAtStart.Length..]);
				}
			}

			if (linesOfInterest.All(o => string.IsNullOrWhiteSpace(o.Content)))
			{
				throw new ManageArgumentsException("The specified config file is fully commented out, empty, or contains only whitespace");
			}

			return linesOfInterest;
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			throw new NotSupportedException();
		}
	}
}
