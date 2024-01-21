
using ProcDumpEx.Utilities;

namespace ProcDumpEx.Commands;

/// <summary>
/// <inheritdoc cref="IActionCommand" />
/// <para>
/// The '-memthdl' parameter extends the existing ProcDump '-ml' parameter. 
/// It allows users to set lower memory thresholds for one or more processes such that if one or more processes falls below the specified memory usage, a memory dump will be generated. 
/// It is also possible to combine this parameter with the '-pn' and '-inf' parameters. The values provided are expressed in units of megabytes (MB).
/// </para>
/// </summary>
internal class CommandMemthdl : IActionCommand
{
	/// <summary>
	/// The constant representing the command name.
	/// </summary>
	public const string CommandName = "-memthdl";

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
	/// Initializes a new instance of the <see cref="CommandMemthdl"/> class
	/// </summary>
	public CommandMemthdl(params string[] values)
	{
	}
}
