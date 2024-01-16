namespace ProcDumpEx.Commands;

internal class CommandInf : ICommand
{
	public const string CommandName = "-inf";

	public string GetCommandName() => CommandName;

	public bool IsRuntimeOption() => true;

	public CommandInf()
	{

	}
}
