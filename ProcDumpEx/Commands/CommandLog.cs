namespace ProcDumpEx.Commands;

internal class CommandLog : ICommand
{
	public const string CommandName = "-log";

	public string GetCommandName() => CommandName;

	public bool IsRuntimeOption() => true;

	public CommandLog()
	{

	}
}
