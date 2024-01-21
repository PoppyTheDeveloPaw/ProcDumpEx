namespace ProcDumpEx.Commands;

/// <summary>
/// Command used as a parameter for the execution of ProcDumpEx. Does not have an executable function but describes the behavior during execution.
/// </summary>
internal interface ICommand
{
	/// <summary>
	/// Gets the name of the command.
	/// </summary>
	/// <returns>The command name</returns>
	public string GetCommandName();

	/// <summary>
	/// Validates the correctness of the initialization parameter of the command.
	/// </summary>
	/// <returns><see langword="True"/> if the parameters are valid; otherwise <see langword="false"/></returns>
	public bool Validate();
}
