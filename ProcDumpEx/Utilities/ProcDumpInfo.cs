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
}
