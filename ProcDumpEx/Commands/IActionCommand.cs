namespace ProcDumpEx.Commands;

/// <summary>
/// The command used as an action during the execution of ProcDumpEx. Implements the Run method, which is used to execute the command.
/// </summary>
internal interface IActionCommand : ICommand
{
	/// <summary>
	/// Method for executing the action of the command async.
	/// </summary>
	public Task RunAsync(Executor executor);

	/// <summary>
	/// Method to stop the execution of the command action async
	/// </summary>
	public Task StopAsync();
}
