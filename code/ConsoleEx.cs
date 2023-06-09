﻿using System.Globalization;
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

		public static void WriteInfo(string infoMessage, string logId) => WriteColor(infoMessage, ConsoleColor.Blue, logId);

		public static void WriteError(string errorMessage, string logId) => WriteColor(errorMessage, ConsoleColor.Red, logId);

		public static void WriteSuccess(string message, string logId) => WriteColor(message, ConsoleColor.Green, logId);

		public static void WriteFailure(string message, string logId) => WriteColor(message, ConsoleColor.DarkYellow, logId);



		private static object _colorLock = new object();
		public static void WriteColor(string message, ConsoleColor color)
		{
			lock (_colorLock)
			{
				Console.ForegroundColor = color;
				WriteLine(message);
				Console.ResetColor();
			}
		}

		public static void WriteColor(string message, ConsoleColor color, string logId)
		{
			string outputMessage = $"{GetIdTime(logId)}{message}";
			WriteColor(outputMessage, color);
		}

		public static void Write(string message, string logId)
		{
			string outputMessage = $"{GetIdTime(logId)}{message}";
			_log.Add(outputMessage);
			Console.Write(outputMessage);
		}

		public static void WriteLine(string message, string logId)
		{
			string outputMessage = $"{GetIdTime(logId)}{message}";
			_log.Add(outputMessage);
			Console.WriteLine(outputMessage);
		}

		public static void WriteLine(string message)
		{
			_log.Add(message);
			Console.WriteLine(message);
		}

		public static void WriteLine()
		{
			string outputMessage = string.Empty;
			_log.Add(outputMessage);
			Console.WriteLine(outputMessage);
		}

		public static void WriteLogFile(string logId) 
		{
			string fileName = $"Log_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt";
			File.WriteAllLines(fileName, _log.Select(x => x.Replace(StartUnderline, "").Replace(EndUnderline, "")));
			ConsoleEx.WriteColor($"Log file saved with the name {fileName}", ConsoleColor.DarkMagenta, logId);
		}

		private static string GetIdTime(string logId) => $"[{logId}][{GetTimeNow()}]: ";

		private static string GetTimeNow() => DateTime.Now.ToString("G", CultureInfo.GetCultureInfo("de-DE"));

		public static void WriteError(string errorMessage, Exception e, string logId)
		{
			StringBuilder sbErrorMessage = new StringBuilder();

			sbErrorMessage.AppendLine(errorMessage);

			if (!(errorMessage.EndsWith('.') || errorMessage.EndsWith('?') || errorMessage.EndsWith('!')))
				sbErrorMessage.Append('.');

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(sbErrorMessage.ToString());

			sb.Append($"Exception: {e}");

			WriteColor(sb.ToString(), ConsoleColor.Red, logId);
		}

		private const string StartUnderline = "\x1B[4m";
		private const string EndUnderline = "\x1B[24m";

		public static void WriteUnderline(string s, string logId)
		{
			var handle = GetStdHandle(STD_OUTPUT_HANDLE);
			uint mode;
			GetConsoleMode(handle, out mode);
			mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
			SetConsoleMode(handle, mode);
			WriteLine($"{StartUnderline}{s}{EndUnderline}", logId);
		}

		private static object _lockObject = new object();
		public static void PrintOutput(ProcDumpInfo info, string[] output, string logId, bool onlyLog = false)
		{
			lock (_lockObject)
			{
				var handle = GetStdHandle(STD_OUTPUT_HANDLE);
				uint mode;
				GetConsoleMode(handle, out mode);
				mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
				SetConsoleMode(handle, mode);
				string firstLine = $"Output of {info.UsedProcDumpFileName} / Process logId: {info.ProcDumpProcessId}. Examined Process: {info.ExaminedProcessName}";

				string dateTime = GetIdTime(logId);
				StringBuilder sb = new StringBuilder();

				sb.AppendLine($"{dateTime}{StartUnderline}{firstLine}{EndUnderline}");

				foreach (var line in output)
				{
					sb.AppendLine($"{dateTime}{line}");
				}

				_log.Add(sb.ToString());

				if (onlyLog)
					return;

				WriteColor(sb.ToString(), ConsoleColor.Cyan);
			}
		}
	}
}
