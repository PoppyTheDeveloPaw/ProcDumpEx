namespace ProcDumpEx.Commands;

/// <summary>
/// The command used as an action during the execution of ProcDumpEx. Implements the Run method, which is used to execute the command.
/// </summary>
internal interface IActionCommand : ICommand
{
	/// <summary>
	/// Method for executing the action of the command.
	/// </summary>
	public void Run();
}
