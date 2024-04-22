namespace ProcDumpEx.Exceptions
{
    public class ProcessNotFoundException(int processId) : Exception
    {
        internal int ProcessId => processId;
    }
}