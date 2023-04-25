using System.Runtime.InteropServices;

namespace ProcDumpEx
{
	internal class KeyEvent
	{
		internal event EventHandler<KeyPressed>? KeyPressedEvent;

		private readonly Thread _thread;

		private static KeyEvent? _singleton;
		public static KeyEvent Instance
		{
			get
			{
				if (_singleton is null)
					_singleton = new KeyEvent();

				return _singleton;
			}
		}

		public KeyEvent()
		{
			Console.CancelKeyPress += Console_CancelKeyPress;

			//Thread to detect if X was pressed
			_thread = new Thread(ThreadMethod);
			_thread.IsBackground = true;
			_thread.Start();
		}

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
			while (true)
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

	enum CtrlType
	{
		CTRL_C_EVENT = 0,
		CTRL_BREAK_EVENT = 1,
		CTRL_CLOSE_EVENT = 2,
		CTRL_LOGOFF_EVENT = 5,
		CTRL_SHUTDOWN_EVENT = 6
	}
}
