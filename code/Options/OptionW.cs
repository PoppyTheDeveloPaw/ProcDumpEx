using ProcDumpEx.code;

namespace ProcDumpEx.Options
{
	[Option("-w", false)]
	public class OptionW : OptionBase
	{
		internal override bool IsCommandCreator => false;

		private readonly ProcessWatcher _processWatcher;
		private ProcDumpExCommand? _command;

		public OptionW()
		{
			_processWatcher = new ProcessWatcher();
			_processWatcher.NewProcess += async (sender, e) => await ProcessWatcher_NewProcessAsync(sender, e);
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			ConsoleEx.WriteInfo($"Until ProcDumpEx is terminated, it waits for new instances of the specified process names ({string.Join(", ", command.ProcessNames)}). For newly started process instances ProcDump is executed with the specified parameters");
			_command = command;
			try
			{
				_processWatcher.Start(command.ProcessNames);
			}
			catch (System.Management.ManagementException e)
			{
				ConsoleEx.WriteError("To use the parameter \"-w\" ProcDumpEx must be started as administrator!", e);
				return Task.FromResult(false);
			}
			return Task.FromResult(true);
		}

		internal override void StopExecution()
		{
			_processWatcher.Stop();
		}

		private async Task ProcessWatcher_NewProcessAsync(object? sender, NewProcessEventArgs e) => await (_command?.ExecuteAsync(e.Process.Id) ?? Task.CompletedTask);
	}
}
