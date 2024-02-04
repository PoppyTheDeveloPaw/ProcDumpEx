using ProcDumpEx.Exceptions;
using System.Diagnostics;

namespace ProcDumpEx.Utilities;

internal class ProcessManager
{
	ManualStopper _stopper;

	private readonly Dictionary<ProcdumpProcessIdentifier, Process> _currentlyMonitoredProcesses;
	private readonly object _lock = new object();

	public ProcessManager(ManualStopper manualStopper)
	{
		_currentlyMonitoredProcesses = new Dictionary<ProcdumpProcessIdentifier, Process>();
		_stopper = manualStopper;

		_stopper.ManualStopEvent += Stopper_ManualStopEvent;
	}

	private void Stopper_ManualStopEvent(object? sender, StopEventArgs e)
	{
		KillAll();
	}

	public async Task StartProcdumpAndWaitForExitAsync(Process process, string args, bool use64Bit, string executionId = "", bool deactivateConsoleLogging = false)
	{
		if (IsCurrentlyMonitored(process.Id, args))
		{
			return;
		}

		string? procdumpPath = GetProcdumpPath(process, use64Bit, executionId);

		if ( string.IsNullOrWhiteSpace( procdumpPath ) )
		{
			return;
		}

		var procdumpProcess = StartProcdumpProcess( procdumpPath, args );

		if (procdumpProcess is null)
		{
			//TODO Handling
			return;
		}

		var info = ProcDumpInfo.GetProcDumpInfo(procdumpProcess, process);
		AddMonitoredProcesses(process, info, executionId);

		bool succeeded = await WaitForExitAndLogOutputAsync(info, procdumpProcess, executionId, deactivateConsoleLogging);

		int numberOfCurrentlyMonitoredProcesses = RemoveMonitoredProcess(info, executionId);

		if (succeeded)
		{
			Logger.AddOutput($"{info.UsedProcDumpFileName} finished successfully. Id: {info.ProcDumpProcessId}, Examined process: {info.ExaminedProcessName}. Number of active monitored processes: {numberOfCurrentlyMonitoredProcesses}", LogType.Success, executionId);
		}
        else
        {
			Logger.AddOutput($"{info.UsedProcDumpFileName} terminated without success. Id: {info.ProcDumpProcessId}, Examined process: {info.ExaminedProcessName}. Number of active monitored processes: {numberOfCurrentlyMonitoredProcesses}", LogType.Failure, executionId);
        }
	}

	private void KillAll()
	{
		if (_currentlyMonitoredProcesses.Any())
		{
			foreach (var itemPair in _currentlyMonitoredProcesses)
			{
				itemPair.Value.Kill();
			}
		}
	}

	private int RemoveMonitoredProcess(ProcDumpInfo info, string executionId)
	{
		lock (_lock)
		{
			_currentlyMonitoredProcesses.Remove(new(info.ExaminedProcessId, info.UsedArguments));
			return _currentlyMonitoredProcesses.Count;
		}
	}

	private async Task<bool> WaitForExitAndLogOutputAsync(ProcDumpInfo info, Process procdumpProcess, string executionId, bool deactivateConsoleLogging)
	{
		string output = "";

		async Task ReadOutput(StreamReader standardOutput)
		{
			output = await standardOutput.ReadToEndAsync();
		}

		await Task.WhenAll(ReadOutput(procdumpProcess.StandardOutput), procdumpProcess.WaitForExitAsync());

		var outputList = output.Split("\r\n");

		Logger.AddProcdumpOutput(info, output.Split("\r\n"), executionId, deactivateConsoleLogging);

		if (output.Contains("Use -? -e to see example command lines."))
		{
			Logger.AddOutput("Procdump help was print, indicating incorrect arguments. Please check specified arguments and if necessary stop ProcDumpEx and restart with correct arguments.", LogType.Failure, executionId);
		}

		return output.Contains("Dump count reached");
	}

	private Process? StartProcdumpProcess(string procdumpPath, string args)
	{
		ProcessStartInfo startInfo = new ProcessStartInfo(procdumpPath)
		{
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			Arguments = args
		};

		return Process.Start(startInfo);
	}

	private void AddMonitoredProcesses(Process process, ProcDumpInfo info, string executionId)
	{
		_currentlyMonitoredProcesses[new(info.ExaminedProcessId, info.UsedArguments)] = process;

		Logger.AddOutput($"{info.UsedProcDumpFileName} started with process id: {info.ProcDumpProcessId} / arguments: {info.UsedArguments}. Examined process: {info.ExaminedProcessName}. Number of active monitored processes: {_currentlyMonitoredProcesses.Count}", executionId: executionId);
	}

	private string? GetProcdumpPath(Process process, bool use64Bit, string executionId)
	{
		try
		{
			var requiredProcdumpVersion = GetRequiredProcdumpVersion(process, use64Bit);
			return Utils.GetProcdumpPath(requiredProcdumpVersion);
		}
		catch (ProcdumpFileMissingException)
		{
			_stopper.ExceptionStop();
			return null;
		}
		catch (GetArchitectureException)
		{
			_stopper.ExceptionStop();
			Logger.AddOutput("An error occurred while querying the process architecture. The program will be terminated. Please create an issue at https://github.com/PoppyTheDeveloPaw/ProcDumpEx/issues with the used parameters", LogType.Error, executionId);
			return null;
		}
		catch (InvalidProcessorArchitecture e)
		{
			_stopper.ExceptionStop();
			Logger.AddException(e.Message, e, executionId);
			return null;
		}
	}

	private ProcdumpVersion GetRequiredProcdumpVersion(Process process, bool use64bit)
	{
		var architecture = process.GetProcessArchitecture();

		return architecture switch
		{
			ProcessorArchitecture.x86 when !use64bit => ProcdumpVersion.x86,
			ProcessorArchitecture.x86 when use64bit => ProcdumpVersion.x64,
			ProcessorArchitecture.AMD64 or ProcessorArchitecture.x64 => ProcdumpVersion.x64,
			ProcessorArchitecture.ARM64 => ProcdumpVersion.x64a,
			_ => throw new InvalidProcessorArchitecture(architecture, process)
		};
	}

	private bool IsCurrentlyMonitored(int processId, string args) => _currentlyMonitoredProcesses.ContainsKey(new ProcdumpProcessIdentifier(processId, args));
}
