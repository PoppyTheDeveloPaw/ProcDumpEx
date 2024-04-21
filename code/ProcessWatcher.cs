using System.Diagnostics;
using System.Management;

namespace ProcDumpEx.code
{
	internal class ProcessWatcher : IDisposable
	{
		private readonly ManagementEventWatcher _managementEventWatcher;
		private List<string>? _relevantProcesses;
		private readonly object _lockObject = new();

		internal event EventHandler<NewProcessEventArgs>? NewProcess;

		public ProcessWatcher()
		{
			_managementEventWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
			_managementEventWatcher.EventArrived += EventArrived;
		}

		/// <summary>
		/// Set the list of relevant processes and start monitoring
		/// </summary>
		/// <param name="relevantProcesses">List of process to monitor.</param>
		internal void Start(List<string>? relevantProcesses = null)
		{
			lock (_lockObject)
			{
				_relevantProcesses = relevantProcesses;
				_managementEventWatcher.Start();
			}
		}

		/// <summary>
		/// Stop monitoring
		/// </summary>
		internal void Stop()
		{
			lock ( _lockObject)
			{
				_managementEventWatcher.Stop();
			}
		}

		/// <summary>
		/// Handles new processes. If these occur in the observed processes (<see cref="_relevantProcesses"/>), a <see cref="NewProcess"/> event is triggered
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventArrived(object sender, EventArrivedEventArgs e)
		{
			// Check if the process name exists
			if (!e.NewEvent.Properties.TryGetValue<string>("ProcessName", out var processName))
			{
				// Get process name failed. Ignore event.
				return;
			}

			// Check if the process id exists
			if (!e.NewEvent.Properties.TryGetValue<int>("ProcessID", out var processId))
			{
				if (!e.NewEvent.Properties.TryGetValue<string>("ProcessID", out var strProcessId))
				{
					// Get process id failed. Process Id could not be interpreted as int or string. Ignore event
					return;
				}

				if (!int.TryParse(strProcessId, out processId))
				{
					// Process id could be read as string, but could not be parsed to int, ignore event 
					return;
				}
			}

			if (_relevantProcesses is not null && !_relevantProcesses.Contains(processName, StringComparer.OrdinalIgnoreCase))
			{
				return;
			}

			try
			{
				var process = Process.GetProcessById(processId);
				NewProcess?.Invoke(this, new NewProcessEventArgs(process));
			}
			catch
			{
				//should never happen
				//ignore
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Stop();
				_managementEventWatcher?.Dispose();
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	internal class NewProcessEventArgs(Process process) : EventArgs
	{
		internal Process Process => process;
	}
}
