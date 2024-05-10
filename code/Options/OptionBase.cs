namespace ProcDumpEx.Options
{
	public abstract class OptionBase
	{
		internal abstract bool IsCommandCreator { get; }
		internal abstract Task<bool> ExecuteAsync(ProcDumpExCommand command);

		internal virtual void StopExecution()
		{
			throw new NotImplementedException();
		}
	}
}
