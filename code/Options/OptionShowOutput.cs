namespace ProcDumpEx.Options
{
	[Option("-showoutput", false)]
	public class OptionShowOutput : OptionBase
	{
		internal override bool IsCommandCreator => false;

		public OptionShowOutput()
		{
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			throw new NotImplementedException();
		}
	}
}
