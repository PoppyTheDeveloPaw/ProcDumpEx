namespace ProcDumpEx.Options
{

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class OptionAttribute : Attribute
	{
		internal string Option { get; set; }
		internal bool ValueExpected { get; set; }

		public OptionAttribute(string option, bool valueExpected) 
		{
			Option = option;
			ValueExpected = valueExpected;
		}
	}
}
