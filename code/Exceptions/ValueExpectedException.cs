using System;

namespace ProcDumpEx.Exceptions
{
    public class ValueExpectedException : Exception
	{
        public ValueExpectedException(string exception, string option) : base(string.Format(exception, option))
        {
        }

        public ValueExpectedException(string exceptionMsg) : base(exceptionMsg)
        {
        } 
    }
}