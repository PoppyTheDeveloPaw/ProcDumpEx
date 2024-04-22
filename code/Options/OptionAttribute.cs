namespace ProcDumpEx.Options
{

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class OptionAttribute(string option, bool valueExpected) : Attribute
	{
		internal string Option => option;
		internal bool ValueExpected => valueExpected;
	}
}
