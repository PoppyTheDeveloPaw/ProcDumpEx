namespace ProcDumpEx.Commands;

internal class CommandMemthd : ICommand
{
	public const string CommandName = "-memthd";

	public string GetCommandName() => CommandName;

	public bool IsRuntimeOption() => false;

	public CommandMemthd(params string[] values)
	{

	}
}
