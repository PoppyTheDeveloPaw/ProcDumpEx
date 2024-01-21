namespace ProcDumpEx.Utilities;

internal class ManualStopper
{
	public event EventHandler<StopEventArgs>? ManualStopEvent;

	internal void SubscribeExitEvents()
	{
		KeyEvent.Instance.KeyPressedEvent += (sender, e) => Instance_KeyPressedEvent(e);
		AppDomain.CurrentDomain.ProcessExit += (sender, e) => ProcessExitEvent();
	}

	internal void ExceptionStop()
	{
		ManualStopEvent?.Invoke(this, new StopEventArgs(StopReason.Exception));
	}

	private void Instance_KeyPressedEvent(KeyPressed e)
	{
		(string key, StopReason reason) = e switch
		{
			KeyPressed.Ctrl_Break => ("CTRL + Break", StopReason.Ctrl_Break),
			KeyPressed.Ctrl_C => ("CTRL + C", StopReason.Ctrl_C),
			KeyPressed.X => ("X", StopReason.X),
			_ => Enum.GetName(e) is string value ? (value, StopReason.Unknown) : ("Unknown", StopReason.Unknown)
		};

		Logger.AddOutput($"Manual termination initiated by pressing \"{key}\".");

		ManualStopEvent?.Invoke(this, new StopEventArgs(reason));
	}

	private void ProcessExitEvent()
	{
		ManualStopEvent?.Invoke(this, new StopEventArgs(StopReason.Closed));
	}
}
