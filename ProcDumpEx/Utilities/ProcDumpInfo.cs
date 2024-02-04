using System;
using System.Diagnostics;

namespace ProcDumpEx.Utilities;

internal struct ProcDumpInfo
{
	internal string UsedProcDumpFileName { get; }
	internal int ProcDumpProcessId { get; }
	internal string UsedArguments { get; }
	internal string ExaminedProcessName { get; }
	internal int ExaminedProcessId { get; }

	internal ProcDumpInfo(string procDump, int procDumpProcessId, string usedArguments, string examinedProcessName, int examinedProcessId)
	{
		UsedProcDumpFileName = Path.GetFileName(procDump);
		ProcDumpProcessId = procDumpProcessId;
		UsedArguments = usedArguments;
		ExaminedProcessName = examinedProcessName;
		ExaminedProcessId = examinedProcessId;
	}
	
	internal static ProcDumpInfo GetProcDumpInfo(Process procdumpProcess, Process monitoredApplicationProcess)
	{
		return new ProcDumpInfo(procdumpProcess.StartInfo.FileName, procdumpProcess.Id, procdumpProcess.StartInfo.Arguments, monitoredApplicationProcess.ProcessName, monitoredApplicationProcess.Id);
	}
}
