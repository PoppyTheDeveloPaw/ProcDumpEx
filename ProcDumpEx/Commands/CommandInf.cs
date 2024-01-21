using ProcDumpEx.Utilities;

namespace ProcDumpEx.Commands;

/// <summary>
/// <inheritdoc cref="ICommand" />
/// <para>
/// The '-inf' parameter in ProcDumpEx ensures that new ProcDump instances are continuously opened until explicitly terminated. 
/// When combined with the '-n' parameter in ProcDump, this means that when the number of dump files specified with '-n' is reached, ProcDump will terminate, and ProcDumpEx will subsequently reopen ProcDump with the same parameters. 
/// However, this parameter can generate a high disk usage and its limit is determined by the overall size and speed of the system.
/// </para>
/// </summary>
internal class CommandInf : ICommand
{
	/// <summary>
	/// The constant representing the command name.
	/// </summary>
	public const string CommandName = "-inf";

	/// <inheritdoc />
	public string GetCommandName() => CommandName;

	/// <inheritdoc />
	public bool Validate(LineInfo? lineInfo) => true;

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandInf"/> class
	/// </summary>
	public CommandInf()
	{

	}
}
