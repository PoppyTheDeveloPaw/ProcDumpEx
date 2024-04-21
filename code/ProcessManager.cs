using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;

namespace ProcDumpEx
{
	internal class ProcessManager
	{
		private readonly Dictionary<ProcDumpProcessIdentifier, Process> _currentMonitoredProcesses;
		private bool _killAllCalled = false;

		private static readonly object _removeLock = new ();

		internal event EventHandler<ProcDumpInfo>? ProcDumpProcessTerminated;

		internal event EventHandler? MonitoringListEmpty;

		public ProcessManager()
		{
			_currentMonitoredProcesses = [];
		}

		public bool IsMonitored(int processId, string arguments) => _currentMonitoredProcesses.ContainsKey(new(processId, arguments));

		public bool IsMonitored(string processName) => _currentMonitoredProcesses.Values.Any(o => Path.GetFileNameWithoutExtension(o.ProcessName) == Path.GetFileNameWithoutExtension(processName));

		public void AddNewMonitoredProcess(int processId, string arguments, Process process, ProcDumpInfo info, string logId) 
		{
			_currentMonitoredProcesses[new(processId, arguments)] = process;
			ConsoleEx.WriteLog($"{info.UsedProcDumpFileName} started with process id: {info.ProcDumpProcessId} / arguments: {info.UsedArguments}. Examined process: {info.ExaminedProcessName}. Number of active monitored processes: {_currentMonitoredProcesses.Count}", logId);
		}

		public bool RemoveMonitoredProcess(int processId, string arguments, ProcDumpInfo info, bool writeIdleMessage, bool succeeded, string logId)
		{
			lock( _removeLock )
			{
				bool value = _currentMonitoredProcesses.Remove(new(processId, arguments));

				if (succeeded)
					ConsoleEx.WriteLog($"{info.UsedProcDumpFileName} finished successfully. Id: {info.ProcDumpProcessId}, Examined process: {info.ExaminedProcessName}. Number of active monitored processes: {_currentMonitoredProcesses.Count}", logId, LogType.Success);
				else
					ConsoleEx.WriteLog($"{info.UsedProcDumpFileName} terminated without success. Id: {info.ProcDumpProcessId}, Examined process: {info.ExaminedProcessName}. Number of active monitored processes: {_currentMonitoredProcesses.Count}", logId, LogType.Failure);

				if (writeIdleMessage && !_killAllCalled && _currentMonitoredProcesses.Count == 0)
					ConsoleEx.WriteLog("Currently all active ProcDump processes have been terminated. ProcDumpEx is idle until new processes are started.", logId, LogType.Info);

				ProcDumpProcessTerminated?.Invoke(this, info);

				if (_currentMonitoredProcesses.Count == 0)
				{
					MonitoringListEmpty?.Invoke(this, EventArgs.Empty);
				}

				return value;
			}
		}

		public bool KillAll()
		{
			_killAllCalled = true;

			if (_currentMonitoredProcesses.Count != 0)
			{
				foreach (var itemPair in _currentMonitoredProcesses)
				{
					itemPair.Value.Kill();
				}
				return false;
			}

			return true;
		}
	}

	internal record ProcDumpProcessIdentifier(int processId, string arguments)
	{
		internal int MonitoredProcess => processId;
		internal string Arguments => arguments;

		public override int GetHashCode() => HashCode.Combine(MonitoredProcess, Arguments);
	}
}
