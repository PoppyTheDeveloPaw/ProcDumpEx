using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;

namespace ProcDumpEx
{
	internal class ProcessManager
	{
		private Dictionary<ProcdumpProcessIdentifier, Process> _currentMonitoredProcesses;
		private bool _killAllCalled = false;

		internal event EventHandler<ProcDumpInfo>? ProcDumpProcessTerminated;

		public ProcessManager()
		{
			_currentMonitoredProcesses = new Dictionary<ProcdumpProcessIdentifier, Process>();
		}

		public bool IsMonitored(int processId, string arguments) => _currentMonitoredProcesses.ContainsKey(new(processId, arguments));

		public bool IsMonitored(string processName) => _currentMonitoredProcesses.Values.Any(o => Path.GetFileNameWithoutExtension(o.ProcessName) == Path.GetFileNameWithoutExtension(processName));

		public void AddNewMonitoredProcess(int processId, string arguments, Process process, ProcDumpInfo info, string logId) 
		{
			_currentMonitoredProcesses[new(processId, arguments)] = process;
			ConsoleEx.WriteLine($"{info.UsedProcDumpFileName} started with process id: {info.ProcDumpProcessId} / arguments: {info.UsedArguments}. Examined process: {info.ExaminedProcessName}. Number of active monitored processes: {_currentMonitoredProcesses.Count}", logId);
		}

		object _removeLock = new object();

		public bool RemoveMonitoredProcess(int processId, string arguments, ProcDumpInfo info, bool writeIdleMessage, bool succeeded, string logId)
		{
			lock( _removeLock )
			{
				bool value = _currentMonitoredProcesses.Remove(new(processId, arguments));

				if (succeeded)
					ConsoleEx.WriteSuccess($"{info.UsedProcDumpFileName} finished successfully. Id: {info.ProcDumpProcessId}, Examined process: {info.ExaminedProcessName}. Number of active monitored processes: {_currentMonitoredProcesses.Count}", logId);
				else
					ConsoleEx.WriteFailure($"{info.UsedProcDumpFileName} terminated without success. Id: {info.ProcDumpProcessId}, Examined process: {info.ExaminedProcessName}. Number of active monitored processes: {_currentMonitoredProcesses.Count}", logId);

				if (writeIdleMessage && !_killAllCalled && !_currentMonitoredProcesses.Any())
					ConsoleEx.WriteInfo("Currently all active ProcDump processes have been terminated. ProcDumpEx is idle until new processes are started.", logId);

				ProcDumpProcessTerminated?.Invoke(this, info);
				return value;
			}
		}

		public void KillAll()
		{
			_killAllCalled = true;
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
