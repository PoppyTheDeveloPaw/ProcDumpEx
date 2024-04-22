using ProcDumpEx.code;

namespace ProcDumpEx.Options
{
	[Option("-w", false)]
	public class OptionW : OptionBase, IDisposable
	{
		internal override bool IsCommandCreator => false;

		private readonly ProcessWatcher _processWatcher;
		private ProcDumpExCommand? _command;

		private string? _logId;

		public OptionW()
		{
			_processWatcher = new ProcessWatcher();
			_processWatcher.NewProcess += ProcessWatcher_NewProcess;
		}

		private async void ProcessWatcher_NewProcess(object? _, NewProcessEventArgs e)
		{
			ConsoleEx.WriteLog($"New process '{e.Process.ProcessName}' ({e.Process.Id}) started.", _logId, LogType.Info);

			if (_command is null)
			{
				return;
			}

			await _command.ExecuteAsync(e.Process.Id);
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			ConsoleEx.WriteLog($"Until ProcDumpEx is terminated, it waits for new instances of the specified process names ({string.Join(", ", command.ProcessNames)}). For newly started process instances ProcDump is executed with the specified parameters", command.LogId, LogType.Info);
			_command = command;
			_logId = command.LogId;
			try
			{
				_processWatcher.Start(command.ProcessNames);
			}
			catch (System.Management.ManagementException e)
			{
				ConsoleEx.WriteException("To use the parameter \"-w\" ProcDumpEx must be started as administrator!", e, command.LogId);
				return Task.FromResult(false);
			}
			return Task.FromResult(true);
		}

		internal override void StopExecution()
		{
			ConsoleEx.WriteLog("Execution of command '-w' is terminated.", _logId, LogType.Info);
			Dispose();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_processWatcher.Dispose();
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
