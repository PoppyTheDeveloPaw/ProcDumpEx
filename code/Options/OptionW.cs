using ProcDumpEx.code;

namespace ProcDumpEx.Options
{
	[Option("-w", false)]
	public class OptionW : OptionBase, IDisposable
	{
		internal override bool IsCommandCreator => false;

		private readonly ProcessWatcher _processWatcher;
		private ProcDumpExCommand? _command;

		public OptionW()
		{
			_processWatcher = new ProcessWatcher();
			_processWatcher.NewProcess += async (_, e) => await ProcessWatcher_NewProcessAsync(e);
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			ConsoleEx.WriteLog($"Until ProcDumpEx is terminated, it waits for new instances of the specified process names ({string.Join(", ", command.ProcessNames)}). For newly started process instances ProcDump is executed with the specified parameters", command.LogId, LogType.Info);
			_command = command;
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
			Dispose();
		}

		private async Task ProcessWatcher_NewProcessAsync(NewProcessEventArgs e) => await (_command?.ExecuteAsync(e.Process.Id) ?? Task.CompletedTask);

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
