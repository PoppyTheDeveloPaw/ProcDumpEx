using System.Management;
using System.Reflection;

namespace ProcDumpEx.Options
{
	[Option("-memthd", true)]
	public class OptionMemthd : OptionBase
	{
		private const string ProcdumpCommand = "-m";
		internal IReadOnlyCollection<int> MemoryCommitThreshold { get; }

		internal override bool IsCommandCreator => true;

		public OptionMemthd(params string[] values)
		{
			double maxMemory = Helper.GetMaxRam();

			List<int> memoryCommitThreshold = [];

			foreach (var value in values)
			{
				GetType().GetCustomAttribute(typeof(OptionAttribute));

				if (!int.TryParse(value, out int mb) || mb < 0 || mb > maxMemory)
				{
					throw new ArgumentException($"{GetType().GetOption()} expects only positive numeric values between 0 and {maxMemory}");
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
