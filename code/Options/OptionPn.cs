namespace ProcDumpEx.Options
{
	[Option("-pn", true)]
	public class OptionPn(params string[] values) : OptionBase
	{
		internal IReadOnlyCollection<string> Processes => values;
		internal override bool IsCommandCreator => false;

		/// <summary>
		/// Is never called
		/// </summary>
		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			throw new NotSupportedException();
		}
	}
}
