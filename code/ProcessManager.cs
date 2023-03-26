using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;

namespace ProcDumpEx
{
	internal class ProcessManager
	{
		private Dictionary<ProcdumpProcessIdentifier, Process> _currentMonitoredProcesses;

		public ProcessManager()
		{
			_currentMonitoredProcesses = new Dictionary<ProcdumpProcessIdentifier, Process>();
		}

		public bool IsMonitored(int processId, string arguments) => _currentMonitoredProcesses.ContainsKey(new(processId, arguments));

		public void AddNewMonitoredProcess(int processId, string arguments, Process process) => _currentMonitoredProcesses[new(processId, arguments)] = process;

		public bool RemoveMonitoredProcess(int processId, string arguments) => _currentMonitoredProcesses.Remove(new(processId, arguments));

		public void KillAll()
		{
			foreach (var itemPair in _currentMonitoredProcesses)
			{
				itemPair.Value.Kill();
			}
			_currentMonitoredProcesses.Clear();
		}
	}

	internal record ProcdumpProcessIdentifier
	{
		internal int MonitoredProcess { get; }
		internal string Arguments { get; }

		public ProcdumpProcessIdentifier(int processId, string arguments)
		{
			MonitoredProcess = processId;
			Arguments = arguments;
		}

		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				int hash = 17;
				// Suitable nullity checks etc, of course :)
				hash = hash * 23 + MonitoredProcess.GetHashCode();
				hash = hash * 23 + Arguments.GetHashCode();
				return hash;
			}
		}
	}
}
