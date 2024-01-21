using ProcDumpEx.Exceptions;
using System.Diagnostics;

namespace ProcDumpEx.Utilities;

internal class ProcessManager
{
	ManualStopper _stopper;

	private Dictionary<ProcdumpProcessIdentifier, Process> _currentlyMonitoredProcesses;

	public async Task StartProcdumpAsync(ProcdumpVersion version, Process process, string args, bool use64Bit, string executionId = "", bool deactivateConsoleLogging = false)
	{
		if (IsCurrentlyMonitored(process.Id, args))
		{
			return;
		}

		string procdumpPath = "";

		try
		{
			var requiredProcdumpVersion = GetRequiredProcdumpVersion(process, use64Bit);
			procdumpPath = Utils.GetProcdumpPath(requiredProcdumpVersion);
		}
		catch (ProcdumpFileMissingException)
		{
			_stopper.ExceptionStop();
			return;
		}
		catch (GetArchitectureException)
		{
			_stopper.ExceptionStop();
			Logger.AddOutput("An error occurred while querying the process architecture. The program will be terminated. Please create an issue at https://github.com/PoppyTheDeveloPaw/ProcDumpEx/issues with the used parameters", LogType.Error, executionId);
			return;
		}
		catch (InvalidProcessorArchitecture e)
		{
			_stopper.ExceptionStop();
			Logger.AddException(e.Message, e, executionId);
			return;
		}

		ProcessStartInfo startInfo = new ProcessStartInfo(procdumpPath)
		{
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			Arguments = args
		};

		Process procdumpProcess = new Process()
		{
			StartInfo = new ProcessStartInfo(procdumpPath)
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				Arguments = args
			}
		};

		if (Process.Start(startInfo) is { } procdump)
		{
			ProcDumpInfo procdumpInfo = new ProcDumpInfo(startInfo.FileName, procdump.Id, startInfo.Arguments, process.ProcessName, process.Id);

			AddMonitoredProcesses(process, procdumpInfo, executionId);

			string output = "";

			async Task ReadOutput(StreamReader standardOutput)
			{
				output = await standardOutput.ReadToEndAsync();
			}

			await Task.WhenAll(ReadOutput(procdump.StandardOutput), procdump.WaitForExitAsync());

			var outputList = output.Split("\r\n");

			Logger.AddProcdumpOutput(procdumpInfo, output.Split("\r\n"), executionId, deactivateConsoleLogging);

			if (output.Contains("Use -? -e to see example command lines."))
			{
				Logger.AddOutput("Procdump help was print, indicating incorrect arguments. Please check specified arguments and if necessary stop ProcDumpEx and restart with correct arguments.", LogType.Failure, executionId);
			}
			
			//TODO Remove
		}
	}

	private void AddMonitoredProcesses(Process process, ProcDumpInfo info, string executionId)
	{
		_currentlyMonitoredProcesses[new(info.ExaminedProcessId, info.UsedArguments)] = process;

		Logger.AddOutput($"{info.UsedProcDumpFileName} started with process id: {info.ProcDumpProcessId} / arguments: {info.UsedArguments}. Examined process: {info.ExaminedProcessName}. Number of active monitored processes: {_currentlyMonitoredProcesses.Count}", executionId: executionId);
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
