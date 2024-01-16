using ProcDumpEx.Utilities;

namespace ProcDumpEx;

internal class Executor
{
	private readonly ParseResult _parseResult;
	private readonly ManualStopper _manualStopper;

	public Executor(ParseResult parseResult, ManualStopper manualStopper)
	{
		_parseResult = parseResult;
		_manualStopper = manualStopper;
		_manualStopper.ManualStopEvent += ManualStopper_ManualStopEvent;
	}

	private void ManualStopper_ManualStopEvent(object? sender, EventArgs e)
	{
		throw new NotImplementedException();
	}

	internal async Task ExecuteAsync()
	{

	}
}
