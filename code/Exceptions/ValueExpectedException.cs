namespace ProcDumpEx.Exceptions
{
	[Serializable]
    internal class ValueExpectedException : Exception
    {
        public ValueExpectedException(string exception, string option) : base(string.Format(exception, option)) { }
	}
}