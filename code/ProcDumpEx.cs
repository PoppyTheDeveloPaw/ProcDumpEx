using ProcDumpEx.Options;

namespace ProcDumpEx;

internal class ProcDumpEx
{
	private readonly ProcDumpExCommand[] _commands;

	private bool _stoppedManually = false;
	private bool _subscribed = false;

	public ProcDumpEx(ProcDumpExCommand[] commands)
	{
		_commands = commands;
		SubscribeEvents();
	}

	private void CurrentDomain_ProcessExit(object? sender, EventArgs e)
	{
		ConsoleEx.WriteLog("ProcDumpEx was terminated manually by closing the window.", "Base", LogType.ShutdownLog);
		StopManually();
	}

	private void Instance_KeyPressedEvent(object? sender, KeyPressed e)
	{
		string key = e switch
		{
			KeyPressed.Ctrl_Break => "CTRL + Break",
			KeyPressed.Ctrl_C => "CTRL + C",
			KeyPressed.X => "X",
			_ => "Unknown"
		};
		ConsoleEx.WriteLog($"ProcDumpEx was terminated manually by pressing the \"{key}\" key", "Base", LogType.ShutdownLog);

		StopManually();
	}

	private void StopManually()
	{
		UnsubscribeEvents();
		_stoppedManually = true;
		foreach (var command in _commands)
		{
			command.Stop();
		}
	}

	public async Task RunAsync()
	{
		if (Array.Exists(_commands, o => o.Help))
		{
			await OptionHelp.WriteHelp("Base");
		}
		else if (!Array.Exists(_commands, o => o.ProcessNames.Count > 0 || o.ProcessIds.Count > 0))
		{
			ConsoleEx.WriteLog("With the specified parameters, neither is waiting for a process to become active, nor is a process to be monitored active. ProcDumpEx is aborted", "Base", LogType.Error);
		}
		else
		{
			List<Task> awaitCommands = [];
			foreach (var command in _commands)
			{
				awaitCommands.Add(command.RunAsync());
			}
			await Task.WhenAll(awaitCommands);

			if (!_stoppedManually)
			{
				ConsoleEx.WriteLog("ProcDumpEx was terminated after everything was done", "Base", LogType.ShutdownLog);
			}
		}
		UnsubscribeEvents();

		if (Array.Exists(_commands, o => o.Log))
		{
			ConsoleEx.SaveLogFile("Base");
		}
	}

	private void SubscribeEvents()
	{
		if (_subscribed)
		{
			return;
		}
		_subscribed = true;
		KeyEvent.Instance.KeyPressedEvent += Instance_KeyPressedEvent;
		AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
	}

	private void UnsubscribeEvents()
	{
		if (!_subscribed)
		{
			return;
		}
		_subscribed = false;
		KeyEvent.Instance.KeyPressedEvent -= Instance_KeyPressedEvent;
		KeyEvent.Instance.Stop();
		AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
	}
}
