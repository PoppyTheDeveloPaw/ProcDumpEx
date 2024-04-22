using System.Diagnostics;

namespace ProcDumpEx.Exceptions
{
	public class InvalidProcessorArchitectureException(ProcessorArchitecture architecture, Process process) 
		: Exception($"The processor architecture {architecture} of the process {Path.GetFileNameWithoutExtension(process.ProcessName)} is not supported. The program is terminated");
}
