using ProcDumpEx.Exceptions;
using ProcDumpEx.Options;
using ProcDumpExExceptions;
using System.Diagnostics;
using System.Text;

namespace ProcDumpEx
{
	internal class ProcDumpExCommand
	{
		internal List<string> ProcessNames { get; }
		internal readonly List<int> ProcessIds;
		internal bool Log { get; }
		internal bool Help { get; }

		internal OptionCfg? OptionCfg { get; }

		internal string LogId { get; }

		private readonly string _baseProcDumpCommand;

		private bool _inf;
		private readonly bool _use64;
		private readonly bool _showoutput;
		private bool _stopCalled = false;

		private readonly List<OptionBase> _procDumpExOptions;

		private readonly TaskCompletionSource _tcs = new TaskCompletionSource();

		private List<string>? _executionProcDumpCommands;
		private List<string> ExecutionProcDumpCommands
		{
			get
			{
				if (_executionProcDumpCommands is not null)
					return _executionProcDumpCommands;

				return new List<string> { _baseProcDumpCommand };
			}
		}

		private readonly ProcessManager _processManager;

		internal ProcDumpExCommand(List<OptionBase> options, List<string> processNames, List<int> processIds, string baseProcDumpCommand, string logId)
		{
			ProcessNames = processNames;

			LogId = logId;
			_procDumpExOptions = options;
			ProcessIds = processIds;
			_baseProcDumpCommand = baseProcDumpCommand;

			_processManager = new ProcessManager();

			_processManager.MonitoringListEmpty += ProcessManager_MonitoringListEmpty;

			Help = _procDumpExOptions.Any(o => o is OptionHelp);

			Log = _procDumpExOptions.Any(o => o is OptionLog);
			if (Log)
				_procDumpExOptions.RemoveAll(o => o is OptionLog);

			_inf = _procDumpExOptions.Any(o => o is OptionInf);
			if (_inf)
				_procDumpExOptions.RemoveAll(o => o is OptionInf);

			_use64 = _procDumpExOptions.Any(o => o is Option64);
			if (_use64)
				_procDumpExOptions.RemoveAll(o => o is Option64);

			_showoutput = _procDumpExOptions.Any(o => o is OptionShowOutput);
			if (_showoutput)
				_procDumpExOptions.RemoveAll(o => o is OptionShowOutput);

			OptionCfg =	_procDumpExOptions.FirstOrDefault(o => o is OptionCfg) as OptionCfg;

			if (!_procDumpExOptions.Any(o => o is OptionW) && OptionCfg is null)
			{
				//Since Wait (-w) is not used, we are only interested in the currently active processes with the name. So we can ignore the names at this point
				foreach (var processName in processNames)
				{
					var processes = GetProcessesByName(processName);

					foreach (var process in processes)
					{
						ProcessIds.Add(process.Id);
					}
				}
				ProcessNames.Clear();
			}

			if (_inf)
				_processManager.ProcDumpProcessTerminated += async (_, e) => await ProcessManager_ProcDumpProcessTerminatedAsync(e);
		}

		internal async Task RunAsync()
		{
			foreach (var creator in _procDumpExOptions.Where(o => o.IsCommandCreator))
				await creator.ExecuteAsync(this);

			foreach (var option in _procDumpExOptions.Where(o => !o.IsCommandCreator))
			{
				if (!await option.ExecuteAsync(this))
				{
					Stop();
					return;
				}
			}

			List<Task> tasks = new List<Task>();

			//Execute for already running processes
			foreach (var processId in ProcessIds)
				tasks.Add(ExecuteAsync(processId));

			foreach (var processName in ProcessNames)
				tasks.Add(ExecuteAsync(processName));

			await Task.WhenAll(tasks);

			if (_procDumpExOptions.Any(o => o is OptionW) || _inf)
				await _tcs.Task;
		}

		internal void Stop()
		{
			if (_tcs.Task.IsCompleted)
				return;

			_stopCalled = true;
			_inf = false;
			_procDumpExOptions.FirstOrDefault(o => o is OptionW)?.StopExecution();

			if (_processManager.KillAll())
			{
				//If no process is currently monitored, the program can be terminated directly
				_tcs.TrySetResult();
			}
		}

		private void ProcessManager_MonitoringListEmpty(object? sender, EventArgs e)
		{
			if (_stopCalled)
				_tcs.TrySetResult();
		}

		internal void AddProcDumpCommand(string command)
		{
			if (_executionProcDumpCommands is null)
				_executionProcDumpCommands = new List<string>();

			_executionProcDumpCommands.Add(string.Join(' ', _baseProcDumpCommand, command));
		}

		private async Task<bool> HelpAsync()
		{
			var option = _procDumpExOptions.FirstOrDefault(o => o is OptionHelp);

			if (option is null)
				return false;

			await option.ExecuteAsync(this);
			return true;
		}

		private Process[] GetProcessesByName(string processName)
		{
			var processes = Process.GetProcessesByName(processName);

			if (!processes.Any())
				processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));

			return processes;
		}

		/// <summary>
		/// Use this method only for the primary start (-pn)
		/// </summary>
		/// <param name="processName"></param>
		private async Task ExecuteAsync(string processName)
		{
			var processes = GetProcessesByName(processName);

			if (!processes.Any())
			{
				StringBuilder sb = new StringBuilder();
				sb.Append($"Currently there is no process with the name {processName} running.");

				if (_procDumpExOptions.Any(o => o is OptionW))
					sb.Append(" ProcDumpEx is idle for this process name until a new process instance is started");
				else
				{
					sb.Append(" The execution for this process name is terminated.");
				}
				
				ConsoleEx.WriteInfo(sb.ToString(), LogId);
				return;
			}

			List<Task> tasks = new List<Task>();

			foreach (var process in processes)
				tasks.Add(StartAllProcDumpCommandsAsync(process));

			await Task.WhenAll(tasks);
		}

		private async Task StartAllProcDumpCommandsAsync(Process process)
		{
			List<Task> tasks = new List<Task>();

			foreach (var argument in ExecutionProcDumpCommands)
				tasks.Add(StartProcDumpAsync(process, argument));

			await Task.WhenAll(tasks);
		}

		internal async Task ExecuteAsync(int processId)
		{
			try
			{
				using var process = Process.GetProcessById(processId) ?? throw new ProcessNotFoundException(processId);
				await StartAllProcDumpCommandsAsync(process);
			}
			catch (Exception e) when (e is (ArgumentException or InvalidOperationException or ProcessNotFoundException))
			{
				ConsoleEx.WriteInfo($"Currently no process is running with the id: {processId}. Execution for this process id is finished", LogId);
				InfRemoveProcessIdentifier(processId);
			}
		}

		/// <summary>
		/// Starts procdump with id and arguments if it is not already running
		/// </summary>
		/// <param name="process"></param>
		/// <param name="argument"></param>
		/// <returns></returns>
		private async Task StartProcDumpAsync(Process process, string argument)
		{
			if (_processManager.IsMonitored(process.Id, argument))
				return;

			ProcessStartInfo info;

			try
			{
				info = new ProcessStartInfo
				{
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					FileName = GetProcDumpPath(process),
					Arguments = argument.Contains(Constants.ProcessPlaceholder) ? argument.Replace(Constants.ProcessPlaceholder, process.Id.ToString()) : string.Join(' ', argument, process.Id)
				};
			}
			catch (ProcDumpFileMissingException e)
			{
				ConsoleEx.WriteError(e.Message, LogId);
				Stop();
				return;
			}
			catch (GetArchitectureException)
			{
				Stop();
				ConsoleEx.WriteError("An error occurred while querying the process architecture. The program will be terminated. Please create an issue at https://github.com/PoppyTheDeveloPaw/ProcDumpEx/issues with the used parameters", LogId);
				return;
			}
			catch (InvalidProcessorArchitecture e)
			{
				Stop();
				ConsoleEx.WriteError(e.Message, LogId);
				return;
			}

			if (Process.Start(info) is { } procdump)
			{
				ProcDumpInfo procDumpInfo = new ProcDumpInfo(info.FileName, procdump.Id, info.Arguments, process.ProcessName, process.Id);

				_processManager.AddNewMonitoredProcess(process.Id, argument, procdump, procDumpInfo, LogId);

				string output = "";

				async Task Test(StreamReader standardOutput)
				{
					output = await standardOutput.ReadToEndAsync();
				}

				await Task.WhenAll(Test(procdump.StandardOutput), procdump.WaitForExitAsync());

				var outputList = output.Split("\r\n");

				ConsoleEx.PrintOutput(procDumpInfo, outputList, LogId, !_showoutput);

				//Check if procdump output contains help string
				if (output.Contains("Use -? -e to see example command lines."))
					ConsoleEx.WriteError("Procdump help was print, indicating incorrect arguments. Please check specified arguments and if necessary stop ProcDumpEx and restart with correct arguments.", LogId);

				_processManager.RemoveMonitoredProcess(process.Id, argument, procDumpInfo, !_inf && _procDumpExOptions.Any(o => o is OptionW), output.Contains("Dump count reached"), LogId);
			}
		}

		private string GetProcDumpPath(Process process)
		{
			var architecture = process.GetProcessArchitecture();
			switch (architecture)
			{
				case ProcessorArchitecture.x86:
					if (_use64)
						return Helper.GetExistingProcDump64Path();
					return Helper.GetExistingProcDumpPath();
				case ProcessorArchitecture.AMD64 or ProcessorArchitecture.x64:
					return Helper.GetExistingProcDump64Path();
				case ProcessorArchitecture.ARM64:
					return Helper.GetExistingProcDump64aPath();
			}

			//Should never happen
			throw new InvalidProcessorArchitecture(architecture, process);
		}
        
		private async Task ProcessManager_ProcDumpProcessTerminatedAsync(ProcDumpInfo e)
		{
			if (_inf)
				await ExecuteAsync(e.ExaminedProcessId);
		}

		private void InfRemoveProcessIdentifier(int processId)
		{
			//If parameter -w is used, inf will be reused at the start of the specified process
			if (_procDumpExOptions.Any(o => o is OptionW))
				return;

			//Only used if parameter -inf is set
			if (!_inf)
				return;

			ProcessIds.Remove(processId);

			if (!ProcessIds.Any())
				_tcs.TrySetResult();
		}
	}

	internal struct ProcDumpInfo
	{
		internal string UsedProcDumpFileName { get; }
		internal int ProcDumpProcessId { get; }
		internal string UsedArguments { get; }
		internal string ExaminedProcessName { get; }
		internal int ExaminedProcessId { get; }

		internal ProcDumpInfo(string procDump, int procDumpProcessId, string usedArguments, string examinedProcessName, int examinedProcessId)
		{
			UsedProcDumpFileName = Path.GetFileName(procDump);
			ProcDumpProcessId = procDumpProcessId;
			UsedArguments = usedArguments;
			ExaminedProcessName = examinedProcessName;
			ExaminedProcessId = examinedProcessId;
		}
	}
}
