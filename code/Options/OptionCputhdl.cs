namespace ProcDumpEx.Options
{
	[Option("-cputhdl", true)]
	public class OptionCputhdl : OptionBase
	{
		private const string ProcdumpCommand = "-cl";
		internal IReadOnlyCollection<int> CpuThreshold { get; }
		internal override bool IsCommandCreator => true;

		public OptionCputhdl(params string[] values)
		{
			List<int> cpuThreshold = new();

			foreach (var value in values)
			{
				if (!int.TryParse(value, out int mb) || mb < 0)
					throw new ArgumentException($"{GetType().GetOption()} expects only positive numeric values");

				cpuThreshold.Add(mb);
			}

			CpuThreshold = cpuThreshold;
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			foreach (var item in CpuThreshold)
				command.AddProcDumpCommand(string.Join(' ', ProcdumpCommand, item));

			return Task.FromResult(true);
		}
	}
}
