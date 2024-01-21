namespace ProcDumpEx.Utilities;

internal class StopEventArgs(StopReason? KeyPressed) : EventArgs
{
}

enum StopReason
{
	Exception,
	Closed,
	Ctrl_C,
	Ctrl_Break,
	X,
	Unknown
}