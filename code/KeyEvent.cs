using System.Runtime.InteropServices;

namespace ProcDumpEx.code
{
	internal class KeyEvent
	{
		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

		private delegate bool EventHandler(CtrlType sig);
		private static event EventHandler? Handler;

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
			if (Handler is null)
			{
				Handler += CtrlTypeEventHandler;
				SetConsoleCtrlHandler(Handler, true);
			}

			_thread = new Thread(ThreadMethod);
			_thread.IsBackground = true;
			_thread.Start();
		}

		private bool CtrlTypeEventHandler(CtrlType sig)
		{
			switch (sig)
			{
				case CtrlType.CTRL_C_EVENT:
					KeyPressedEvent?.Invoke(this, KeyPressed.Ctrl_C);
					break;
				case CtrlType.CTRL_BREAK_EVENT:
					KeyPressedEvent?.Invoke(this, KeyPressed.Ctrl_Break);
					break;
			}
			return true;
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
