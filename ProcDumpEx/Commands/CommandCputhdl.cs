namespace ProcDumpEx.Commands;

/// <summary>
/// <inheritdoc cref="IActionCommand" />
/// <para>
/// Parameter cputhdl extends the -cl parameter of Procdump. 
/// This parameter checks if one or more processes have fallen below a certain CPU usage and then creates memory dumps. 
/// The cputhdl parameter allows the user to set one or more values to monitor and create memory dumps accordingly. 
/// It is also possible to combine this parameter with the '-pn' and '-inf' parameters. 
/// The values provided are expressed in units of % (percent).
/// </para>
/// </summary>
internal class CommandCputhdl : IActionCommand
{
	/// <summary>
	/// The constant representing the command name.
	/// </summary>
	public const string CommandName = "-cputhdl";

	/// <inheritdoc />
	public string GetCommandName() => CommandName;

	/// <inheritdoc />
	public void Run()
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public bool Validate()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandCputhdl"/> class
	/// </summary>
	public CommandCputhdl(params string[] values)
	{

	}
}
