using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
namespace ProcDumpEx.Utilities;

/// <summary>
/// Logger class
/// </summary>
internal static class Logger
{
	/// <summary>
	/// List of all log entries
	/// </summary>
	private static List<string> _log = new List<string>();

	/// <summary>
	/// The standard output device. Initially, this is the active console screen buffer
	/// </summary>
	private const int STD_OUTPUT_HANDLE = -11;

	/// <summary>
	/// When writing with WriteFile or WriteConsole, characters are parsed for VT100 and similar control character sequences that control 
	/// cursor movement, color/font mode, and other operations that can also be performed via the existing Console APIs. 
	/// For more information, see Console Virtual Terminal Sequences. Ensure ENABLE_PROCESSED_OUTPUT is set when using this flag.
	/// </summary>
	private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

	/// <summary>
	/// Value to start an underlined string in console.
	/// </summary>
	private const string StartUnderline = "\x1B[4m";

	/// <summary>
	/// Value to end an underlined string in console.
	/// </summary>
	private const string EndUnderline = "\x1B[24m";

	/// <summary>
	/// Retrieves a handle to the specified standard device (standard input, standard output, or standard error).
	/// </summary>
	/// <param name="nStdHandle">The standard device. This parameter can be one of the following values.</param>
	/// <returns>
	/// If the function succeeds, the return value is a handle to the specified device, or a redirected handle set by a previous call to SetStdHandle. 
	/// The handle has GENERIC_READ and GENERIC_WRITE access rights, unless the application has used SetStdHandle to set a standard handle with lesser access.
	/// If the function fails, the return value is INVALID_HANDLE_VALUE. To get extended error information, call GetLastError.
	/// If an application does not have associated standard handles, such as a service running on an interactive desktop, and has not redirected them, the return value is NULL.
	/// </returns>
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern nint GetStdHandle(int nStdHandle);

	/// <summary>
	/// Retrieves the current input mode of a console's input buffer or the current output mode of a console screen buffer.
	/// </summary>
	/// <param name="hConsoleHandle">A handle to the console input buffer or the console screen buffer. The handle must have the GENERIC_READ access right. For more information, see Console Buffer Security and Access Rights.</param>
	/// <param name="lpMode">
	/// A pointer to a variable that receives the current mode of the specified buffer.
	/// If the hConsoleHandle parameter is an input handle, the mode can be one or more of the following values. When a console is created, all input modes except ENABLE_WINDOW_INPUT and ENABLE_VIRTUAL_TERMINAL_INPUT are enabled by default.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero. 
	/// If the function fails, the return value is zero.To get extended error information, call GetLastError.
	/// </returns>
	[DllImport("kernel32.dll")]
	static extern bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

	/// <summary>
	/// Sets the input mode of a console's input buffer or the output mode of a console screen buffer.
	/// </summary>
	/// <param name="hConsoleHandle">A handle to the console input buffer or a console screen buffer. The handle must have the GENERIC_READ access right. For more information, see Console Buffer Security and Access Rights.</param>
	/// <param name="dwMode">
	/// The input or output mode to be set.
	/// If the hConsoleHandle parameter is an input handle, the mode can be one or more of the following values. When a console is created, all input modes except ENABLE_WINDOW_INPUT and ENABLE_VIRTUAL_TERMINAL_INPUT are enabled by default.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.To get extended error information, call GetLastError.</returns>
	[DllImport("kernel32.dll")]
	static extern bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

	/// <summary>
	/// Lock object for thread safety logging.
	/// </summary>
	private static object _logLock = new object();

	/// <summary>
	/// Adds the given log message to the log (console and log array).
	/// </summary>
	/// <param name="message">Message to log.</param>
	/// <param name="logType">Type of the log message.</param>
	/// <param name="executionId">Id to keep the output of several executors apart</param>
	/// <param name="deactivateConsoleLogging">Parameter with which the output on the console can be deactivated. Is false by default.</param>
	public static void AddOutput(string message, LogType logType = LogType.Default, string executionId = "", bool deactivateConsoleLogging = false)
	{
		lock (_logLock)
		{
			string time = GetTimeNow();
			AddToLogList(message, time, logType, executionId);
			if (!deactivateConsoleLogging)
			{
				WriteConsoleOutput(message, time, logType, executionId);
			}
		}
	}

	/// <summary>
	/// Adds an empty line to the log.
	/// </summary>
	public static void AddEmptyLine()
	{
		lock (_logLock)
		{
			_log.Add(string.Empty);
			Console.WriteLine();
		}
	}

	/// <summary>
	/// Adds an underlined line to the log.
	/// </summary>
	/// <param name="message">Message to log.</param>
	/// <param name="logType">Type of the log message.</param>
	/// <param name="executionId">Id to keep the output of several executors apart</param>
	/// <param name="deactivateConsoleLogging">Parameter with which the output on the console can be deactivated. Is false by default.</param>
	public static void AddUnderlinedOutput(string message, LogType logType = LogType.Default, string executionId = "", bool deactivateConsoleLogging = false)
	{
		lock (_logLock)
		{
			string time = GetTimeNow();
			var handle = GetStdHandle(STD_OUTPUT_HANDLE);
			uint mode;
			GetConsoleMode(handle, out mode);
			mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
			SetConsoleMode(handle, mode);
			AddToLogList(message, time, logType);
			if (!deactivateConsoleLogging)
			{
				WriteConsoleOutput($"{StartUnderline}{message}{EndUnderline}", time, logType);
			}
		}
	}

	/// <summary>
	/// Adds an exception to the output.
	/// </summary>
	/// <param name="message">Description message of the exception.</param>
	/// <param name="exception">Exception which is to be logged.</param>
	/// <param name="executionId">Id to keep the output of several executors apart</param>
	public static void AddException(string message, Exception exception, string executionId = "")
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine(message);
		if (!(message.EndsWith('.') || message.EndsWith('?') || message.EndsWith('!')))
		{
			sb.Append('.');
		}
		sb.AppendLine("Exception: ");
		sb.Append(exception);
		AddOutput(sb.ToString(), LogType.Error, executionId);
	}

	/// <summary>
	/// Writes the message to the console.
	/// </summary>
	/// <param name="message">Message to be logged.</param>
	/// <param name="time">Timestamp of the log message.</param>
	/// <param name="logType">Type of the log message.</param>
	private static void WriteConsoleOutput(string message, string time, LogType logType, string executionId = "")
	{
		try
		{
			if (logType != LogType.Default)
			{
				Console.ForegroundColor = GetLoggingColor(logType);
			}

			StringBuilder sb = new StringBuilder();
			sb.AppendPreParameter(executionId);
			sb.AppendPreParameter(time);
			sb.Append(": ");
			sb.Append(message);
			Console.WriteLine(sb.ToString());
		}
		finally
		{
			Console.ResetColor();
		}
	}

	/// <summary>
	/// Adds the log entry to the log list.
	/// </summary>
	/// <param name="message">Message to be logged.</param>
	/// <param name="time">Timestamp of the log message.</param>
	/// <param name="logType">Type of the log message.</param>
	private static void AddToLogList(string message, string time, LogType logType, string executionId = "")
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendPreParameter(executionId);
		sb.AppendPreParameter(Enum.GetName(typeof(LogType), logType) ?? "");
		sb.AppendPreParameter(time);
		sb.Append(": ");
		sb.Append(message);
		_log.Add(sb.ToString());
	}

	/// <summary>
	/// Adds an parameter to the given string builder, e.g. "[parameter]".
	/// </summary>
	/// <param name="sb">StringBuilder for which the parameter is to be added.</param>
	/// <param name="parameter">Parameter which is to be added.</param>
	private static void AppendPreParameter(this StringBuilder sb, string parameter)
	{
		sb.Append('[');
		sb.Append(parameter);
		sb.Append(']');
	}

	/// <summary>
	/// Returns the console logging color for the given log type.
	/// </summary>
	/// <param name="logType">Log type for which the color is required.</param>
	/// <returns>Logging color of the given log type.</returns>
	private static ConsoleColor GetLoggingColor(LogType logType) => logType switch
	{
		LogType.Info => ConsoleColor.Blue,
		LogType.Error => ConsoleColor.Red,
		LogType.Success => ConsoleColor.Green,
		LogType.Failure => ConsoleColor.DarkYellow,
		LogType.LogFileSaved or LogType.Exit => ConsoleColor.DarkMagenta,
		LogType.ProcdumpOutput => ConsoleColor.Cyan,
		_ => ConsoleColor.White
	};

	public static void WriteLogFile()
	{
		string fileName = $"Log_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt";
		File.WriteAllLines(fileName, _log);
		AddOutput($"Log file saved with the name {fileName}", LogType.LogFileSaved);
	}

	private static string GetTimeNow() => DateTime.Now.ToString("G", CultureInfo.GetCultureInfo("de-DE"));

	public static void AddProcdumpOutput(ProcDumpInfo info, string[] output, string executionId, bool deactivateConsoleLogging = false)
	{
		lock (_logLock)
		{
			string timestamp = GetTimeNow();

			var handle = GetStdHandle(STD_OUTPUT_HANDLE);
			uint mode;
			GetConsoleMode(handle, out mode);
			mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
			SetConsoleMode(handle, mode);

			string firstLine = $"Output of {info.UsedProcDumpFileName} / Process logId: {info.ProcDumpProcessId}. Examined Process: {info.ExaminedProcessName}";
			AddToLogList(firstLine, timestamp, LogType.ProcdumpOutput, executionId);
			if (!deactivateConsoleLogging)
			{
				WriteConsoleOutput($"{StartUnderline}{firstLine}{EndUnderline}", timestamp, LogType.ProcdumpOutput, executionId);
			}

			foreach (var line in output)
			{
				AddToLogList(line, timestamp, LogType.ProcdumpOutput, executionId);
				if (!deactivateConsoleLogging)
				{
					WriteConsoleOutput(line, timestamp, LogType.ProcdumpOutput, executionId);
				}
			}
		}
	}

	//private static object _lockObject = new object();
	//public static void PrintOutput(ProcDumpInfo info, string[] output, string logId, bool onlyLog = false)
	//{
	//	lock (_lockObject)
	//	{
	//		var handle = GetStdHandle(STD_OUTPUT_HANDLE);
	//		uint mode;
	//		GetConsoleMode(handle, out mode);
	//		mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
	//		SetConsoleMode(handle, mode);
	//		string firstLine = $"Output of {info.UsedProcDumpFileName} / Process logId: {info.ProcDumpProcessId}. Examined Process: {info.ExaminedProcessName}";

	//		string dateTime = GetIdTime(logId);
	//		StringBuilder sb = new StringBuilder();

	//		sb.AppendLine($"{dateTime}{StartUnderline}{firstLine}{EndUnderline}");

	//		foreach (var line in output)
	//		{
	//			sb.AppendLine($"{dateTime}{line}");
	//		}

	//		_log.Add(sb.ToString());

	//		if (onlyLog)
	//			return;

	//		WriteColor(sb.ToString(), ConsoleColor.Cyan);
	//	}
	//}
}
