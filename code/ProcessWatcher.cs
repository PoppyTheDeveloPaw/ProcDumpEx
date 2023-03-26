using System.Diagnostics;
using System.Management;

namespace ProcDumpEx.code
{
	internal class ProcessWatcher
	{
		private readonly ManagementEventWatcher _managementEventWatcher;
		private List<string>? _relevantProcesses;

		internal event EventHandler<NewProcessEventArgs>? NewProcess;

		public ProcessWatcher()
		{
			_managementEventWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
			_managementEventWatcher.EventArrived += EventArrived;
		}

		internal void Start(List<string>? relevantProcesses = null)
		{
			_relevantProcesses = relevantProcesses;
			_managementEventWatcher.Start();
		}

		internal void Stop()
		{
			_managementEventWatcher.Stop();
		}

		private void EventArrived(object sender, EventArrivedEventArgs e)
		{
			//TODO Check if processId is a string or an int
			if (!e.NewEvent.Properties.TryGetValue<string>("ProcessName", out var processName) || !e.NewEvent.Properties.TryGetValue<int>("ProcessID", out var processId))
				return;

			if (_relevantProcesses is not null && !_relevantProcesses.Contains(processName, StringComparer.OrdinalIgnoreCase))
				return;

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
	}

	internal class NewProcessEventArgs : EventArgs
	{
		internal Process Process { get; }

		public NewProcessEventArgs(Process process)
		{
			Process = process;
		}
	}
}
