namespace ProcDumpEx.Options
{
	[Option("-memthdl", true)]
	public class OptionMemthdl : OptionBase
	{
		private const string ProcdumpCommand = "-ml";
		internal IReadOnlyCollection<int> MemoryCommitThreshold  { get; }
		internal override bool IsCommandCreator => true;

		public OptionMemthdl(params string[] values)
		{
			double maxMemory = Helper.GetMaxRam();

			List<int> memoryCommitThreshold = [];

			foreach (var value in values)
			{
				if (!int.TryParse(value, out int mb) || mb < 0 || mb > maxMemory)
				{
					throw new ArgumentException($"{GetType().GetOption()} expects only positive numeric values between 0 MB and {maxMemory} MB");
				}

				memoryCommitThreshold.Add(mb);
			}

			MemoryCommitThreshold = memoryCommitThreshold;
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			foreach (var item in MemoryCommitThreshold)
				command.AddProcDumpCommand(string.Join(' ', ProcdumpCommand, item));

			return Task.FromResult(true);
		}
	}
}
