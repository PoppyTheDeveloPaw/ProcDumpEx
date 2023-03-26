namespace ProcDumpEx.Options
{
	[Option("-64", true)]
	internal class Option64 : OptionBase
	{
		internal override bool IsCommandCreator => false;

		public Option64()
		{
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			throw new NotImplementedException();
		}
	}
}
