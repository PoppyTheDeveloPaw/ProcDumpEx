namespace ProcDumpEx.Commands;

internal class CommandW : ICommand
{
	public const string CommandName = "-w";

	public string GetCommandName() => CommandName;

	public bool IsRuntimeOption() => false;

	public CommandW()
	{

	}
}
