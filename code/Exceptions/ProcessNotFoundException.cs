namespace ProcDumpEx.Exceptions
{
    internal class ProcessNotFoundException : Exception
    {
        internal int ProcessId { get; }

        public ProcessNotFoundException(int processId)
        {
            ProcessId = processId;
        }
    }
}