
using ProcDumpEx.Utilities;
using System.Text;

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

	/// <summary>
	/// Contains the command that is required for the ProcDump execution
	/// </summary>
	private const string ProcdumpCommand = "-cl";

	private string[] _cpuUsageValues;

	/// <inheritdoc />
	public string GetCommandName() => CommandName;

	/// <inheritdoc />
	public bool Validate(LineInfo? lineInfo)
	{
		StringBuilder sb = new StringBuilder();
		if (lineInfo is not null)
		{
			sb.Append($"{lineInfo}: ");
		}

		if (_cpuUsageValues.Length == 0)
		{
			sb.Append($"{CommandName}: When using {CommandName}, at least one parameter must be specified!");
			Logger.AddOutput(sb.ToString(), logType: LogType.Error);
			return false;
		}
		else if (_cpuUsageValues.Select(o => int.TryParse(o, out int n) && n > 0).Any(o => o == false))
		{
			sb.Append($"{CommandName}: When using {CommandName}, only positive numerical integer values may be used!");
			Logger.AddOutput(sb.ToString(), logType: LogType.Error);
			return false;
		}

		return true;
	}

	/// <inheritdoc />
	public Task RunAsync(Executor executor)
	{
		foreach (var cpuUsageValue in _cpuUsageValues)
		{
			executor.AddProcDumpCommand(string.Join(' ', ProcdumpCommand, cpuUsageValue));
		}
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task StopAsync()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandCputhdl"/> class
	/// </summary>
	public CommandCputhdl(params string[] values)
	{
		_cpuUsageValues = values;
	}
}
