namespace ProcDumpEx.Exceptions
{
    public class DuplicateOptionsException(IEnumerable<string> duplicateKeys) : Exception
    {
        internal IEnumerable<string> DuplicateKeys => duplicateKeys;
    }
}