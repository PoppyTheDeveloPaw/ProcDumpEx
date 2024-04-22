namespace ProcDumpEx.Exceptions
{
    public class ValueExpectedException(string exception, string option) : Exception(string.Format(exception, option));
}