using ProcDumpEx;
using ProcDumpEx.Commands;
using ProcDumpEx.Utilities;

//if (!Utils.CheckPreconditions())
//{
//	return;
//}

if (!args.Any())
{
	Logger.AddOutput($"For the execution of ProcDumpEx parameters are expected. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters.", logType: LogType.Error);
	return;
}

ArgumentParser parser = new ArgumentParser(args);

if (parser.GetParsingResult() is not { } parsingResult)
{
	return;
}

ManualStopper manualStopper = new ManualStopper();
manualStopper.SubscribeExitEvents();

List<Task> awaitCommands = new List<Task>();
foreach (var result in parsingResult)
{
	awaitCommands.Add(new Executor(result, manualStopper).ExecuteAsync());
}
await Task.WhenAll(awaitCommands);

Logger.AddOutput("ProcDumpEx was terminated. Everything was finished.", logType: LogType.Exit);

if (parsingResult.Any(o => o.Commands.Any(o => o is CommandLog)))
{
	Logger.WriteLogFile();
}