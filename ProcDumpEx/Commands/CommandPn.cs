using ProcDumpEx.Utilities;

namespace ProcDumpEx.Commands;


/// <summary>
/// <inheritdoc cref="ICommand" />
/// <para>
/// If the parameter '-pn' is used with a process name, ProcDumpEx retrieves the process IDs (PIDs) of all programmes 
/// and their corresponding subordinate processes with the same name and opens a separate ProcDump instance for each of them. 
/// It is also possible to directly specify the ID of a process instead of its name. In connection with the parameter '-inf', 
/// however, this can only be restarted as long as the process with the specified ID exists. The parameter '-pn' is an optional parameter, as it is also 
/// possible to specify the process names and process IDs at the end of the arguments. If it is necessary that the PID is inserted at a certain 
/// position and not at the end of the arguments, this can be achieved with the placeholder [ProcessPlaceholder].
/// </para>
/// </summary>
internal class CommandPn : ICommand
{
	/// <summary>
	/// The constant representing the command name.
	/// </summary>
	public const string CommandName = "-pn";

	/// <inheritdoc />
	public string GetCommandName() => CommandName;

	/// <inheritdoc />
	public bool Validate()
	{
		if (!Processes.Any())
		{
			Logger.AddOutput("-pn: The -pn command expects at least one parameter", logType: LogType.Error);
			return false;
		}
		return true;
	}

	/// <summary>
	/// List of process names and process IDs specified using the -pn parameter
	/// </summary>
	public IReadOnlyList<string> Processes;

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandPn"/> class
	/// </summary>
	public CommandPn(params string[] values)
	{
		Processes = values;
	}
}
