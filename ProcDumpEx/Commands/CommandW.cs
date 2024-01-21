namespace ProcDumpEx.Commands;

/// <summary>
/// <inheritdoc cref="IActionCommand" />
/// <para>
/// The '-w' parameter is a parameter used by ProcDump itself. When used in conjunction with ProcDumpEx, it allows ProcDumpEx to detect 
/// the termination and restart of a process, and re-open ProcDump instances for the processes found using the '-pn' parameter and process name. 
/// This ensures that the desired processes are constantly monitored even after termination and restart.
/// </para>
/// </summary>
internal class CommandW : IActionCommand
{
	/// <summary>
	/// The constant representing the command name.
	/// </summary>
	public const string CommandName = "-w";

	/// <inheritdoc />
	public string GetCommandName() => CommandName;

	/// <inheritdoc />
	public void Run()
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public bool Validate() => true;

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandW"/> class
	/// </summary>
	public CommandW()
	{
	}
}
