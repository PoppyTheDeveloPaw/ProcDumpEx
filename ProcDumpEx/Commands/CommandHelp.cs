namespace ProcDumpEx.Commands;

/// <summary>
/// <inheritdoc cref="IActionCommand" />
/// <para>
/// The -help parameter can be used to display explanations and examples for the individual parameters supported by ProcDumpEx. In addition, the help of ProcDump is printed. 
/// As soon as the parameter '-help' occurs in the specified arguments, nothing is executed but only the help of ProcDumpEx and ProcDump is displayed.
/// </para>
/// </summary>
internal class CommandHelp : IActionCommand
{
	/// <summary>
	/// The constant representing the command name.
	/// </summary>
	public const string CommandName = "-help";

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
	/// Initializes a new instance of the <see cref="CommandHelp"/> class
	/// </summary>
	public CommandHelp()
	{

	}
}
