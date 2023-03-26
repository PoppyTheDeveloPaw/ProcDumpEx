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
			Console.WriteLine(infoMessage);
			Console.ResetColor();
		}

		public static void WriteError(string errorMessage)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(errorMessage);
			Console.ResetColor();
		}

		public static void WriteError(string errorMessage, Exception e)
		{
			StringBuilder sbErrorMessage = new StringBuilder();

			sbErrorMessage.AppendLine(errorMessage);

			if (!(errorMessage.EndsWith('.') || errorMessage.EndsWith('?') || errorMessage.EndsWith('!')))
				sbErrorMessage.Append('.');

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(sbErrorMessage.ToString());

			sb.Append($"Excepiton: {e}");

			WriteError(sb.ToString());
		}

		public static void WriteUnderline(string s)
		{
			var handle = GetStdHandle(STD_OUTPUT_HANDLE);
			uint mode;
			GetConsoleMode(handle, out mode);
			mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
			SetConsoleMode(handle, mode);
			Console.WriteLine($"\x1B[4m{s}\x1B[24m");
		}

		private static object _lockObject = new object();
		public static void PrintOutput(string procdumpFileName, int procdumpProcessId, string examinedProcessName, string output)
		{
			lock (_lockObject)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				var handle = GetStdHandle(STD_OUTPUT_HANDLE);
				uint mode;
				GetConsoleMode(handle, out mode);
				mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
				SetConsoleMode(handle, mode);
				string firstLine = $"Output of {procdumpFileName} / Process id: {procdumpProcessId}. Examined Process: {examinedProcessName}";
				Console.WriteLine($"\x1B[4m{firstLine}\x1B[24m\n{output}");
				Console.ResetColor();
			}
		}
	}
}
