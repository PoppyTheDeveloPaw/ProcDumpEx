namespace ProcDumpEx.Commands;

internal class CommandCputhd : ICommand
{
	public const string CommandName = "-cputhd";

	public string GetCommandName() => CommandName;

	public bool IsRuntimeOption() => false;

	public CommandCputhd(params string[] values)
	{

	}
}
