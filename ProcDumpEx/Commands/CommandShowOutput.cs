namespace ProcDumpEx.Commands;

internal class CommandShowOutput : ICommand
{
	public const string CommandName = "-showoutput";

	public string GetCommandName() => CommandName;

	public bool IsRuntimeOption() => true;

	public CommandShowOutput()
	{

	}
}
