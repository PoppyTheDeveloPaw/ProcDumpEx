using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace ProcDumpEx
{
	internal static class ConsoleEx
	{
		const int STD_OUTPUT_HANDLE = -11;
		const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll")]
		static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

		[DllImport("kernel32.dll")]
		static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

		public static void WriteInfo(string infoMessage)
		{
			Console.ForegroundColor = ConsoleColor.Blue;
			WriteLine(infoMessage);
			Console.ResetColor();
		}

		public static void WriteError(string errorMessage)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			WriteLine(errorMessage);
			Console.ResetColor();
		}

		public static void WriteSuccess(string message)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			WriteLine(message);
			Console.ResetColor();
		}

		public static void WriteFailure(string message)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			WriteLine(message);
			Console.ResetColor();
		}

		public static void WriteColor(string message, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			WriteLine(message);
			Console.ResetColor();
		}

		public static void Write(string message) => Console.Write($"[{GetTimeNow()}]: {message}");
		public static void WriteLine(string message) => Console.WriteLine($"[{GetTimeNow()}]: {message}");
		public static void WriteLine() => Console.WriteLine(string.Empty);

		private static string GetTimeNow() => DateTime.UtcNow.ToString("G", CultureInfo.GetCultureInfo("de-DE"));

	public static void WriteError(string errorMessage, Exception e)
		{
			StringBuilder sbErrorMessage = new StringBuilder();

			sbErrorMessage.AppendLine(errorMessage);

			if (!(errorMessage.EndsWith('.') || errorMessage.EndsWith('?') || errorMessage.EndsWith('!')))
				sbErrorMessage.Append('.');

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(sbErrorMessage.ToString());

			sb.Append($"Exception: {e}");

			WriteError(sb.ToString());
		}

		public static void WriteUnderline(string s)
		{
			var handle = GetStdHandle(STD_OUTPUT_HANDLE);
			uint mode;
			GetConsoleMode(handle, out mode);
			mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
			SetConsoleMode(handle, mode);
			WriteLine($"\x1B[4m{s}\x1B[24m");
		}

		private static object _lockObject = new object();
		public static void PrintOutput(ProcDumpInfo info, string output)
		{
			lock (_lockObject)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				var handle = GetStdHandle(STD_OUTPUT_HANDLE);
				uint mode;
				GetConsoleMode(handle, out mode);
				mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
				SetConsoleMode(handle, mode);
				string firstLine = $"Output of {info.UsedProcDumpFileName} / Process id: {info.ProcDumpProcessId}. Examined Process: {info.ExaminedProcessName}";
				WriteLine($"\x1B[4m{firstLine}\x1B[24m\n{output}");
				Console.ResetColor();
			}
		}
	}
}
