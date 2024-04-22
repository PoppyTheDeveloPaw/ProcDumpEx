using System.Runtime.InteropServices;

namespace ProcDumpEx
{
	internal class KeyEvent
	{
		internal event EventHandler<KeyPressed>? KeyPressedEvent;

		private bool _run = true;
		private static readonly object _lock = new();

		private static KeyEvent? _singleton;
		public static KeyEvent Instance
		{
			get
			{
				if (_singleton is null)
				{
					lock (_lock)
					{
						_singleton ??= new KeyEvent();
					}
				}
				return _singleton;
			}
		}

		public KeyEvent()
		{
			Console.CancelKeyPress += Console_CancelKeyPress;

			//Thread to detect if X was pressed
			var thread = new Thread(ThreadMethod)
			{
				IsBackground = true
			};
			thread.Start();
		}

		public void Stop() => _run = false;

		private void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
		{
			e.Cancel = true;

			switch (e.SpecialKey)
			{
				case ConsoleSpecialKey.ControlC:
					KeyPressedEvent?.Invoke(this, KeyPressed.Ctrl_C);
					break;
				case ConsoleSpecialKey.ControlBreak:
					KeyPressedEvent?.Invoke(this, KeyPressed.Ctrl_Break);
					break;
			}
		}

		private void ThreadMethod()
		{
			while (_run)
			{
				if (Console.ReadKey(true).Key == ConsoleKey.X)
				{
					KeyPressedEvent?.Invoke(this, KeyPressed.X);
				}
			}
		}
	}

	enum KeyPressed
	{
		Ctrl_C,
		Ctrl_Break,
		X
	}
}
