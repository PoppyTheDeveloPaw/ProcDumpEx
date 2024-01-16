namespace ProcDumpEx.Commands;

internal class CommandHelp : ICommand
{
	public const string CommandName = "-help";

	public string GetCommandName() => CommandName;

	public bool IsRuntimeOption() => true;

	public CommandHelp()
	{

	}
}
