using ProcDumpEx.Commands;
using ProcDumpEx.Utilities;
using System.Security.Cryptography.X509Certificates;

namespace ProcDumpEx;

internal class Executor
{
	internal ProcDumpExCommand ParsedCommand { get; }

	private readonly ManualStopper _manualStopper;
	private readonly TaskCompletionSource _taskCompletionSource;

	private List<string>? _procdumpCommandsToExecute; 

	public Executor(ProcDumpExCommand parsedCommand, ManualStopper manualStopper)
	{
		ParsedCommand = parsedCommand;
		_manualStopper = manualStopper;
		_taskCompletionSource = new TaskCompletionSource();
		_manualStopper.ManualStopEvent += ManualStopper_ManualStopEvent;
	}

	private void ManualStopper_ManualStopEvent(object? sender, EventArgs e)
	{
		_taskCompletionSource.TrySetResult();
	}

	private IEnumerable<string> GetProcDumpArgs(string process)
	{
		var commands = _procdumpCommandsToExecute ?? new List<string> { ParsedCommand.BaseProcdumpCommand };

		var commandsToExecute = new List<string>();

		foreach (var command in commands)
		{
			if (command.Contains(Constants.ProcessPlaceholder, StringComparison.OrdinalIgnoreCase))
			{
				commandsToExecute.Add(command.Replace(Constants.ProcessPlaceholder, process, StringComparison.OrdinalIgnoreCase));
			}
			else
			{
				commandsToExecute.Add(string.Join(' ', command, process));
			}
		}

		return commandsToExecute;
	}

	internal void AddProcDumpCommand(string command)
	{
		if (_procdumpCommandsToExecute is null)
		{
			_procdumpCommandsToExecute = new List<string>();
		}
		_procdumpCommandsToExecute.Add(command);
	}

	internal async Task<EndType> RunAsync()
	{
		return EndType.Manually_X;
	}

	internal static async Task<EndType> ExecuteAsync(IReadOnlyList<ProcDumpExCommand> commands, ManualStopper manualStopper)
	{
		if (commands.SelectMany(o => o.Commands).FirstOrDefault(o => o is CommandHelp) is CommandHelp help)
		{
			await help.RunAsync(null!);
			return EndType.Normal;
		}
		else
		{
			List<Task<EndType>> awaitCommands = new List<Task<EndType>>();
			foreach (var command in commands)
			{
				awaitCommands.Add(new Executor(command, manualStopper).RunAsync());
			}

			return (await Task.WhenAll(awaitCommands)).FirstOrDefault(o => o != EndType.Normal, EndType.Normal);
		}
	}

	internal static void ExecuteLog(IReadOnlyList<ProcDumpExCommand> commands)
	{
		if (commands.SelectMany(o => o.Commands).Any(o => o is CommandLog))
		{
			Logger.WriteLogFile();
		}
	}
}
