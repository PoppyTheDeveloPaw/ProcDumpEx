using ProcDumpEx.Exceptions;
using ProcDumpEx.Options;
using System.Diagnostics;
using System.Text;

namespace ProcDumpEx
{
	internal class ProcDumpExCommand
	{
		internal List<string> ProcessNames { get; }

		private readonly string _baseProcDumpCommand;
		private readonly List<int> _processIds;

		private bool _inf;
		private readonly bool _use64;
		private readonly bool _showoutput;

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

		internal ProcDumpExCommand(List<OptionBase> options, List<string> processNames, List<int> processIds, string baseProcDumpCommand)
		{
			ProcessNames = processNames;

			_procDumpExOptions = options;
			_processIds = processIds;
			_baseProcDumpCommand = baseProcDumpCommand;

			_processManager = new ProcessManager();

			_inf = _procDumpExOptions.Any(o => o is OptionInf);
			if (_inf)
				_procDumpExOptions.RemoveAll(o => o is OptionInf);

			_use64 = _procDumpExOptions.Any(o => o is Option64);
			if (_use64)
				_procDumpExOptions.RemoveAll(o => o is Option64);

			_showoutput = _procDumpExOptions.Any(o => o is OptionShowOutput);
			if (_showoutput)
				_procDumpExOptions.RemoveAll(o => o is OptionShowOutput);
		}

		internal async Task RunAsync()
		{
			if (await HelpAsync())
				return;

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
			foreach (var processId in _processIds)
				tasks.Add(ExecuteAsync(processId));

			foreach (var processName in ProcessNames)
				tasks.Add(ExecuteAsync(processName));

			await Task.WhenAll(tasks);

			if (_procDumpExOptions.Any(o => o is (OptionW or OptionInf)))
				await _tcs.Task;
		}

		internal void Stop()
		{
			if (_tcs.Task.IsCompleted)
				return;

			_inf = false;
			_procDumpExOptions.FirstOrDefault(o => o is OptionW)?.StopExecution();
			_processManager.KillAll();

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

		/// <summary>
		/// Use this method only for the primary start (-pn)
		/// </summary>
		/// <param name="processName"></param>
		private async Task ExecuteAsync(string processName)
		{
			var processes = Process.GetProcessesByName(processName);

			if (!processes.Any())
				processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));

			if (!processes.Any())
			{
				StringBuilder sb = new StringBuilder();
				sb.Append($"Currently there is no process with the name {processName} running.");

				if (_procDumpExOptions.Any(o => o is OptionW))
					sb.Append(" Waiting until a process with this name is started.");
				else
					sb.Append(" The execution for this process name is terminated.");

				ConsoleEx.WriteInfo(sb.ToString());
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
				ConsoleEx.WriteInfo($"Currently no process is running with the id: {processId}. Execution for this process id is finished");
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

			ProcessStartInfo info = new ProcessStartInfo
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				FileName = _use64 ? Constants.FullProcdump64Path : process.Is64Bit() ? Constants.FullProcdump64Path : Constants.FullProcdumpPath,
				Arguments = argument.Contains(Constants.ProcessPlaceholder) ? argument.Replace(Constants.ProcessPlaceholder, process.Id.ToString()) : string.Join(' ', argument, process.Id)
			};

			if (Process.Start(info) is { } procdump)
			{
				string processName = process.ProcessName;

				_processManager.AddNewMonitoredProcess(process.Id, argument, procdump);
				Console.WriteLine($"{Path.GetFileName(info.FileName)} started with process id: {procdump.Id} / arguments: {info.Arguments}");

				string output = "";

				async Task Test(StreamReader standardOutput)
				{
					output = await standardOutput.ReadToEndAsync();
				}

				await Task.WhenAll(Test(procdump.StandardOutput), procdump.WaitForExitAsync());

				_processManager.RemoveMonitoredProcess(process.Id, argument);

				//Check if procdump output contains help string
				if (output.Contains("Use -? -e to see example command lines."))
					ConsoleEx.WriteError("Procdump help was print, indicating incorrect arguments. Please check specified arguments and if necessary stop ProcDumpEx and restart with correct arguments.");

				if (_showoutput)
					ConsoleEx.PrintOutput(Path.GetFileName(info.FileName), procdump.Id, processName, output);

				Console.WriteLine($"{Path.GetFileName(info.FileName)} finished. Id: {procdump.Id}, Examined process: {processName}");

				if (_inf)
					await ExecuteAsync(procdump.Id);
			}
		}
	}
}
