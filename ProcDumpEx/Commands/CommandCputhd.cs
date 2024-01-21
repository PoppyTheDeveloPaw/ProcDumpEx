namespace ProcDumpEx.Commands;

/// <summary>
/// <inheritdoc cref="IActionCommand" />
/// <para>
/// The cputhd parameter extends the -c parameter of Procdump. 
/// It checks whether one or more processes have exceeded a certain CPU usage and then generates memory dumps. 
/// The cputhd parameter allows the user to set one or more values to monitor and generate memory dumps accordingly. 
/// It is also possible to combine this parameter with the '-pn' and '-inf' parameters. 
/// The values provided are expressed in units of % (percent).
/// </para>
/// </summary>
internal class CommandCputhd : IActionCommand
{
	/// <summary>
	/// The constant representing the command name.
	/// </summary>
	public const string CommandName = "-cputhd";

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
	/// Initializes a new instance of the <see cref="CommandCputhd"/> class
	/// </summary>
	public CommandCputhd(params string[] values)
	{

	}
}
