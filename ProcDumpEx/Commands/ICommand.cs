namespace ProcDumpEx.Commands;

internal interface ICommand
{
    public string GetCommandName();
    public bool IsRuntimeOption();
}
