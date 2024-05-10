namespace ProcDumpEx.Options
{
	[Option("-log", false)]
	internal class OptionLog : OptionBase
	{
		internal override bool IsCommandCreator => false;

		public OptionLog()
		{
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			throw new NotImplementedException();
		}
	}
}
