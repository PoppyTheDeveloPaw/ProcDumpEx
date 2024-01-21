using Gapotchenko.FX.Diagnostics;
using ProcDumpEx.Exceptions;
using System.Collections.Specialized;
using System.Diagnostics;

namespace ProcDumpEx
{
	internal static class ProcessEx
	{
		private const string ProcessorArchitecture = "PROCESSOR_ARCHITECTURE";

		internal static ProcessorArchitecture GetProcessArchitecture(this Process process)
		{
			var dictionary = process.ReadEnvironmentVariables();

			if (!dictionary.TryGetValue(ProcessorArchitecture, out var value))

				if (string.IsNullOrEmpty(value))
					throw new GetArchitectureException();

			try
			{
				return (ProcessorArchitecture)Enum.Parse(typeof(ProcessorArchitecture), value!, true);
			}
			catch
			{
				//Ugly as fuck - Unexpected architecture received
				throw new GetArchitectureException();
			}
		}

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
	}

	enum ProcessorArchitecture
	{
		x86,
		AMD64,
		x64,
		ARM,
		ARM64,
	}
}
