namespace ProcDumpEx.Options
{
	[Option("-pn", true)]
	public class OptionPn : OptionBase
	{
		internal IReadOnlyCollection<string> Processes;
		internal override bool IsCommandCreator => false;

		public OptionPn(params string[] values)
		{
			Processes = values;
		}

		/// <summary>
		/// Is never called
		/// </summary>
		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			throw new NotImplementedException();
		}
	}
}
