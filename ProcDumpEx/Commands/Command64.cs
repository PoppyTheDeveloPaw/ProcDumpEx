namespace ProcDumpEx.Commands;

internal class Command64 : ICommand
{
	public const string CommandName = "-64";

	public string GetCommandName() => CommandName;

	public bool IsRuntimeOption() => true;

	public Command64()
	{
	}
}
