using ProcDumpEx.Utilities;

namespace ProcDumpEx.Commands;

/// <summary>
/// <inheritdoc cref="ICommand" />
/// <para>
/// Since ProcDumpEx starts several ProcDump instances at the same time, the normal output of ProcDump is suppressed and only 
/// displayed when a ProcDump instance has been started or terminated. With the parameter -showoutput, the output of the ProcDump 
/// instances is displayed after they have been terminated.
/// </para>
/// </summary>
internal class CommandShowOutput : ICommand
{
	/// <summary>
	/// The constant representing the command name.
	/// </summary>
	public const string CommandName = "-showoutput";

	/// <inheritdoc />
	public string GetCommandName() => CommandName;

	/// <inheritdoc />
	public bool Validate(LineInfo? lineInfo) => true;

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandShowOutput"/> class
	/// </summary>
	public CommandShowOutput()
	{

	}
}
