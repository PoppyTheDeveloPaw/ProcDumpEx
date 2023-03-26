namespace ProcDumpEx.Options
{
	[Option("-inf", false)]
	public class OptionInf : OptionBase
	{
		internal override bool IsCommandCreator => false;

		public OptionInf()
		{
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			throw new NotImplementedException();
		}
	}
}
