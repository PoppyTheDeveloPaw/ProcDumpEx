namespace ProcDumpEx.Exceptions
{
    [Serializable]
    internal class NoValueExpectedException : Exception
    {
        public string Option { get; }

        public NoValueExpectedException(string option)
        {
            Option = option;
        }
    }
}