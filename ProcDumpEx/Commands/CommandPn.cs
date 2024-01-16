namespace ProcDumpEx.Commands;

internal class CommandPn : ICommand
{
	public const string CommandName = "-pn";

	public string GetCommandName() => CommandName;

	public bool IsRuntimeOption() => true;

	public IReadOnlyList<string> Processes;

	public CommandPn(params string[] values)
	{
		Processes = values;
	}
}
