﻿namespace ProcDumpEx.Options
{
	[Option("-cputhd", true)]
	public class OptionCputhd : OptionBase
	{
		private const string ProcdumpCommand = "-c";
		internal IReadOnlyCollection<int> CpuThreshold { get; }
		internal override bool IsCommandCreator => true;

		public OptionCputhd(params string[] values)
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
