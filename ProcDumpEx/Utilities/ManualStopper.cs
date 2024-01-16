namespace ProcDumpEx.Utilities;

internal class ManualStopper
{
	public event EventHandler? ManualStopEvent;

	internal void SubscribeExitEvents()
	{
		KeyEvent.Instance.KeyPressedEvent += (sender, e) => Instance_KeyPressedEvent(e);
		AppDomain.CurrentDomain.ProcessExit += (sender, e) => ProcessExitEvent();
	}

	private void Instance_KeyPressedEvent(KeyPressed e)
	{
		string key = e switch
		{
			KeyPressed.Ctrl_Break => "CTRL + Break",
			KeyPressed.Ctrl_C => "CTRL + C",
			KeyPressed.X => "X",
			_ => Enum.GetName(e) ?? "Unknown"
		};

		Logger.AddOutput("Manual termination initiated by pressing \"{key}\".");

		ManualStopEvent?.Invoke(this, EventArgs.Empty);
	}

	private void ProcessExitEvent()
	{
		ManualStopEvent?.Invoke(this, EventArgs.Empty);
	}
}
