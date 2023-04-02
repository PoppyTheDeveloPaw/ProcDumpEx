using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace ProcDumpEx
{
	internal static class ConsoleEx
	{
		private static List<string> _log = new List<string>();

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

		public static void Write(string message)
		{
			string outputMessage = $"[{GetTimeNow()}]: {message}";
			_log.Add(outputMessage);
			Console.Write(outputMessage);
		}

		public static void WriteLine(string message, bool onlyLog = false)
		{
			string outputMessage = $"[{GetTimeNow()}]: {message}";
			_log.Add(outputMessage);
			if (!onlyLog)
				Console.WriteLine(outputMessage);
		}

		public static void WriteLine()
		{
			string outputMessage = string.Empty;
			_log.Add(outputMessage);
			Console.WriteLine(outputMessage);
		}

		public static void WriteLogFile() 
		{
			string fileName = $"Log_{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}.txt";
			File.WriteAllLines(fileName, _log.Select(x => x.Replace(StartUnderline, "").Replace(EndUnderline, "")));
			ConsoleEx.WriteColor($"Log file saved with the name {fileName}", ConsoleColor.DarkMagenta);
		}

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

		private const string StartUnderline = "\x1B[4m";
		private const string EndUnderline = "\x1B[24m";

		public static void WriteUnderline(string s)
		{
			var handle = GetStdHandle(STD_OUTPUT_HANDLE);
			uint mode;
			GetConsoleMode(handle, out mode);
			mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
			SetConsoleMode(handle, mode);
			WriteLine($"{StartUnderline}{s}{EndUnderline}");
		}

		private static object _lockObject = new object();
		public static void PrintOutput(ProcDumpInfo info, string output, bool onlyLog = false)
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
				WriteLine($"{StartUnderline}{firstLine}{EndUnderline}\n{output}", onlyLog);
				Console.ResetColor();
			}
		}
	}
}
