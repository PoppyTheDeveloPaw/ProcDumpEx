using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace ProcDumpEx;

internal enum LogType
{
	Info,
	Error,
	Success,
	Failure,
	Normal,
	ShutdownLog,
	ProcDump
}

internal static partial class ConsoleEx
{
	private static readonly List<string> _log = [];
	private static readonly object _colorLock = new();
	private static readonly object _lockObject = new();

	[LibraryImport("kernel32.dll", SetLastError = true)]
	private static partial IntPtr GetStdHandle(int nStdHandle);

	[LibraryImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

	[LibraryImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

	/// <summary>
	/// Returns the console color for the given log type.
	/// </summary>
	/// <param name="logType"></param>
	/// <returns></returns>
	private static ConsoleColor GetConsoleColor(LogType logType) => logType switch
	{
		LogType.Info => ConsoleColor.Blue,
		LogType.Error => ConsoleColor.Red,
		LogType.Success => ConsoleColor.Green,
		LogType.Failure => ConsoleColor.DarkYellow,
		LogType.ShutdownLog => ConsoleColor.DarkMagenta,
		LogType.ProcDump => ConsoleColor.Cyan,
		_ => ConsoleColor.White
	};

	/// <summary>
	/// Writes the given exception to the console and adds it into the log.
	/// </summary>
	/// <param name="errorMessage"></param>
	/// <param name="e"></param>
	/// <param name="logId"></param>
	public static void WriteException(string errorMessage, Exception e, string logId)
	{
		StringBuilder sbErrorMessage = new();

		sbErrorMessage.AppendLine(errorMessage);

		if (!(errorMessage.EndsWith('.') || errorMessage.EndsWith('?') || errorMessage.EndsWith('!')))
			sbErrorMessage.Append('.');

		StringBuilder sb = new();
		sb.AppendLine(sbErrorMessage.ToString());

		sb.Append($"Exception: {e}. Inner exception: {e.InnerException}");

		WriteLog(sb.ToString(), logId, LogType.Error);
	}

	/// <summary>
	/// Writes an empty line.
	/// </summary>
	public static void WriteEmptyLine() => WriteLog(string.Empty);

	/// <summary>
	/// Adds the given log message to the console and the log.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="logId"></param>
	/// <param name="logType"></param>
	/// <param name="writeLog"></param>
	/// <param name="writeConsole"></param>
	public static void WriteLog(string message, string? logId = null, LogType logType = LogType.Normal, bool writeLog = true, bool writeConsole = true) =>
		ExecuteLog(GetLogMessage(message, logId), GetConsoleColor(logType), logType, writeLog, writeConsole);

	/// <summary>
	/// Creates the log message.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="logId"></param>
	/// <returns></returns>
	private static string GetLogMessage(string message, string? logId)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return message;
		}

		return AddPrefixToLines(message, GetPrefixString(logId));
	}

	/// <summary>
	/// Adds the given prefix to the log message
	/// </summary>
	/// <param name="input"></param>
	/// <param name="prefix"></param>
	/// <returns></returns>
	private static string AddPrefixToLines(string input, string prefix)
	{
		string[] lines = input.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

		StringBuilder sb = new();
		foreach (string line in lines)
		{
			sb.Append(prefix);
			sb.AppendLine(line);
		}

		return sb.ToString();
	}

	/// <summary>
	/// Creates the prefix [time][log id]
	/// </summary>
	/// <param name="logId"></param>
	/// <returns></returns>
	private static string GetPrefixString(string? logId)
	{
		StringBuilder sb = new();
		sb.Append($"[{DateTime.Now.ToString("G", CultureInfo.GetCultureInfo("de-DE"))}]");
		if (!string.IsNullOrWhiteSpace(logId))
		{
			sb.Append($"[{logId}]");
		}
		sb.Append(": ");
		return sb.ToString();
	}

	/// <summary>
	/// Writes the message into the log and to the console.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="color"></param>
	/// <param name="logType"></param>
	/// <param name="writeLog"></param>
	/// <param name="writeConsole"></param>
	private static void ExecuteLog(string message, ConsoleColor color, LogType logType, bool writeLog, bool writeConsole)
	{
		void Execute()
		{
			if (writeLog)
			{
				AddToLog(message, logType);
			}
			if (writeConsole)
			{
				Console.WriteLine(message);
			}
		}

		if (string.IsNullOrWhiteSpace(message))
		{
			// Empty line needs no color change
			Execute();
			return;
		}

		lock (_colorLock)
		{
			Console.ForegroundColor = color;
			Execute();
			Console.ResetColor();
		}
	}

	/// <summary>
	/// Adds the message into the log.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="logType"></param>
	private static void AddToLog(string message, LogType logType)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			_log.Add(message);
		}
		else
		{
			string logMsg = message.Replace(Constants.StartUnderline, string.Empty).Replace(Constants.EndUnderline, string.Empty);
			_log.Add(AddPrefixToLines(logMsg, $"[{Enum.GetName(logType)}]"));
		}
	}

	/// <summary>
	/// Writes the log into an logfile
	/// </summary>
	/// <param name="logId"></param>
	public static void SaveLogFile(string logId)
	{
		string fileName = $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
		File.WriteAllLines(
			fileName,
			_log.Select(x => x.Replace(Constants.StartUnderline, string.Empty).Replace(Constants.EndUnderline, string.Empty)));

		WriteLog($"Log file saved with the name {fileName}", logId, LogType.ShutdownLog);
	}

	/// <summary>
	/// Writes the test underlined to the console.
	/// </summary>
	/// <param name="s"></param>
	/// <param name="logId"></param>
	public static void WriteUnderline(string s, string logId)
	{
		var handle = GetStdHandle(Constants.StdOutputHandle);
		GetConsoleMode(handle, out uint mode);
		mode |= Constants.EnableVirtualTerminalProcessing;
		SetConsoleMode(handle, mode);

		WriteLog($"{Constants.StartUnderline}{s}{Constants.EndUnderline}", logId);
	}

	/// <summary>
	/// Prints the procdump output to the console and the log.
	/// </summary>
	/// <param name="info"></param>
	/// <param name="output"></param>
	/// <param name="logId"></param>
	/// <param name="onlyLog"></param>
	public static void PrintOutput(ProcDumpInfo info, string[] output, string logId, bool onlyLog = false)
	{
		lock (_lockObject)
		{
			var handle = GetStdHandle(Constants.StdOutputHandle);
			GetConsoleMode(handle, out uint mode);
			mode |= Constants.EnableVirtualTerminalProcessing;
			SetConsoleMode(handle, mode);
			string firstLine = $"Output of {info.UsedProcDumpFileName} / Process logId: {info.ProcDumpProcessId}. Examined Process: {info.ExaminedProcessName}";

			StringBuilder sb = new();
			sb.AppendLine($"{Constants.StartUnderline}{firstLine}{Constants.EndUnderline}");

			foreach (var line in output)
			{
				sb.AppendLine($"{line}");
			}

			WriteLog(sb.ToString(), logId, LogType.ProcDump, writeConsole: !onlyLog);
		}
	}
}
