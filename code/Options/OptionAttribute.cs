namespace ProcDumpEx.Options
{

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class OptionAttribute(string option, bool valueExpected, 
		string? parameterMissingExcetionMessage = null) : Attribute
	{
		internal string Option => option;
		internal bool ValueExpected => valueExpected;
		internal string ParameterMissingExceptionMsg
		{
			get
			{
				if (string.IsNullOrWhiteSpace(parameterMissingExcetionMessage))
				{
					return $"For the option {option} one or more values are expected";
				}
				return parameterMissingExcetionMessage;
			}
		}
	}
}
