namespace ProcDumpEx.Commands;

internal class CommandMemthdl : ICommand
{
	public const string CommandName = "-memthdl";

	public string GetCommandName() => CommandName;

	public bool IsRuntimeOption() => false;

	public CommandMemthdl(params string[] values)
	{

	}
}
