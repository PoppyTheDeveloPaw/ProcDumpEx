using ProcDumpEx.Utilities;

namespace ProcDumpEx.Commands;

/// <summary>
/// <inheritdoc cref="ICommand" />
/// <para>
/// ProcDump internally checks whether the process to be monitored is a 32- or 64-bit application and starts procdump.exe or procdump64.exe depending on the process. With the parameter -64, this check is bypassed and the 64-bit variant is always used.
/// </para>
/// </summary>
internal class Command64 : ICommand
{
	/// <summary>
	/// The constant representing the command name.
	/// </summary>
	public const string CommandName = "-64";

	/// <inheritdoc />
	public string GetCommandName() => CommandName;

	/// <inheritdoc />
	public bool Validate(LineInfo? lineInfo) => true;

	/// <summary>
	/// Initializes a new instance of the <see cref="Command64"/> class
	/// </summary>
	public Command64()
	{
	}
}
