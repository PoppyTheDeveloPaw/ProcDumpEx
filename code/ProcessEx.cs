using Gapotchenko.FX.Diagnostics;
using ProcDumpExExceptions;
using System.Collections.Specialized;
using System.Diagnostics;

namespace ProcDumpEx
{
	internal static class ProcessEx
	{
		private const string ProcessorArchitecture = "PROCESSOR_ARCHITECTURE";

		private static bool TryGetValue(this StringDictionary dict, string key, out string? value)
		{
			value = null;
			if (dict.ContainsKey(key))
			{
				value = dict[key];
				return true;
			}
			return false;
		}

		/// <summary>
		/// Extension method for the process class to get the processor architecture
		/// </summary>
		/// <param name="process"></param>
		/// <returns></returns>
		/// <exception cref="GetArchitectureException">Thrown if an error occurs during the determination of the processor architecturer</exception>
		internal static ProcessorArchitecture GetProcessArchitecture(this Process process)
		{
			StringDictionary dictionary;
			try
			{
				dictionary = process.ReadEnvironmentVariables();
			}
			catch (Exception e)
			{
				//Ugly as fuck - Not possible to read process environment variables.
				ConsoleEx.WriteException("Failed to get process architecture. The process may require higher privileges than administrator.", e, string.Empty);
				throw new GetArchitectureException();
			}

			if (!dictionary.TryGetValue(ProcessorArchitecture, out var value))
			{
				ConsoleEx.WriteLog($"No value '{ProcessorArchitecture}' exists in the process environment variables dictionary.", logType: LogType.Error);
				throw new GetArchitectureException();
			}

			if (string.IsNullOrWhiteSpace(value))
			{
				ConsoleEx.WriteLog("The value for processor architecture is null or empty.", logType: LogType.Error);
				throw new GetArchitectureException();
			}

			try
			{
				return (ProcessorArchitecture)Enum.Parse(typeof(ProcessorArchitecture), value, true);
			}
			catch (Exception e)
			{
				//Ugly as fuck - Unexpected architecture received
				ConsoleEx.WriteException($"Unexpected architecture received. Value: {value}", e, string.Empty);
				throw new GetArchitectureException();
			}
		}
	}

	public enum ProcessorArchitecture
	{
		x86,
		AMD64,
		x64,
		ARM,
		ARM64,
	}
}
