namespace ProcDumpEx.Exceptions
{
	[Serializable]
    internal class DuplicateOptionsException : Exception
    {
        internal IEnumerable<string> DuplicateKeys { get; }

        public DuplicateOptionsException(IEnumerable<string> duplicateKeys)
        {
            DuplicateKeys = duplicateKeys;
        }
    }
}