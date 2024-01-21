namespace ProcDumpEx.Commands;

/// <summary>
/// <inheritdoc cref="ICommand" />
/// <para>
/// If the -log parameter is specified, a log file is written when ProcDumpEx exits, which contains the complete output of the console.
/// </para>
/// </summary>
internal class CommandLog : ICommand
{
	/// <summary>
	/// The constant representing the command name.
	/// </summary>
	public const string CommandName = "-log";

	/// <inheritdoc />
	public string GetCommandName() => CommandName;

	/// <inheritdoc />
	public bool Validate()=> true;

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandLog"/> class
	/// </summary>
	public CommandLog()
	{

	}
}
