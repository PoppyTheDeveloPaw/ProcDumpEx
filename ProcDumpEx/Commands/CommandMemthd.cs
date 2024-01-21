
using ProcDumpEx.Utilities;

namespace ProcDumpEx.Commands;

/// <summary>
/// <inheritdoc cref="IActionCommand" />
/// <para>
/// The 'memthd' parameter extends the ProcDump '-m' parameter. 
/// By using the 'memthd' parameter, users can set multiple memory thresholds for one or more processes without having to manually open ProcDump instances. 
/// It is also possible to combine this parameter with the '-pn' and '-inf' parameters. The values provided are expressed in units of megabytes (MB).
/// </para>
/// </summary>
internal class CommandMemthd : IActionCommand
{
	/// <summary>
	/// The constant representing the command name.
	/// </summary>
	public const string CommandName = "-memthd";

	/// <inheritdoc />
	public string GetCommandName() => CommandName;

	/// <inheritdoc />
	public bool Validate(LineInfo? lineInfo)
	{
		return true;
		//TODO
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public async Task RunAsync(Executor executor)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public async Task StopAsync()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandMemthd"/> class
	/// </summary>
	public CommandMemthd(params string[] values)
	{

	}
}
