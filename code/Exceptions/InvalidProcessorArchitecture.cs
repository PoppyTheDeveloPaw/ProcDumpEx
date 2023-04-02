using System.Diagnostics;

namespace ProcDumpEx.Exceptions
{
	internal class InvalidProcessorArchitecture : Exception
	{
		internal InvalidProcessorArchitecture(ProcessorArchitecture architecture, Process process) : base($"The processor architecture {architecture} of the process {Path.GetFileNameWithoutExtension(process.ProcessName)} is not supported. The program is terminated")
		{

		}
	}
}
