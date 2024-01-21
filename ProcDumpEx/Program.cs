using ProcDumpEx;
using ProcDumpEx.Commands;
using ProcDumpEx.Utilities;
using System.Threading.Tasks.Dataflow;

if (!args.Any())
{
	Logger.AddOutput($"For the execution of ProcDumpEx parameters are expected. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
	return;
}

//if (!Utils.CheckPreconditions())
//{
//	return;
//}

ArgumentParser parser = new ArgumentParser(args);

if (parser.GetParsingResult() is not { } parsedCommands)
{
	return;
}

ManualStopper manualStopper = new ManualStopper();
manualStopper.SubscribeExitEvents();

string logText = await Executor.ExecuteAsync(parsedCommands, manualStopper) switch
{
	EndType.Normal => "ProcDumpEx was terminated after everything was done.",
	EndType.Manually_Ctrl_Break => "ProcDumpEx was terminated manually by pressing: \"CTRL + Break\".",
	EndType.Manually_Ctrl_C => "ProcDumpEx was terminated manually by pressing: \"CTRL + C\".",
	EndType.Manually_X => "ProcDumpEx was terminated manually by pressing: \"X\".",
	EndType.Manually_Closed => "ProcDumpEx was terminated by closing.",
	_ => "ProcDumpEx has been terminated. Reason is unknown."
};

Logger.AddOutput(logText, logType: LogType.Exit);

Executor.ExecuteLog(parsedCommands);