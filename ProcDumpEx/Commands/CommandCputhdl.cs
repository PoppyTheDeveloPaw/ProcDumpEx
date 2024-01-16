namespace ProcDumpEx.Commands;

internal class CommandCputhdl : ICommand
{
	public const string CommandName = "-cputhdl";

	public string GetCommandName() => CommandName;

	public bool IsRuntimeOption() => false;

	public CommandCputhdl(params string[] values)
	{

	}
}
