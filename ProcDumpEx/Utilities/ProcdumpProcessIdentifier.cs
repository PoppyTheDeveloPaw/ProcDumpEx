namespace ProcDumpEx.Utilities;

internal record ProcdumpProcessIdentifier(int MonitoredProcessId, string Arguments)
{
	public override int GetHashCode()
	{
		unchecked // Overflow is fine, just wrap
		{
			int hash = 17;
			// Suitable nullity checks etc, of course :)
			hash = hash * 23 + MonitoredProcessId.GetHashCode();
			hash = hash * 23 + Arguments.GetHashCode();
			return hash;
		}
	}
}