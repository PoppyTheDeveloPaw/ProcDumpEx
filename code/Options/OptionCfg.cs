using ProcDumpEx.Exceptions;
using ProcDumpEx.Options;

namespace ProcDumpEx.Options
{
	[Option("-cfg", true)]
	internal class OptionCfg : OptionBase
	{
		internal string FilePath { get; }
		internal override bool IsCommandCreator => false;

		public OptionCfg(string path)
		{
			FilePath = path;
		}

		internal string[] GetArgumentsFromFile()
		{
			if (!string.IsNullOrEmpty(FilePath))
			{
				if (!File.Exists(FilePath))
					throw new ManageArgumentsException("The specified config path is invalid");
				return File.ReadAllLines(FilePath);
			}
			return new string[0];
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			throw new NotImplementedException();
		}
	}
}
