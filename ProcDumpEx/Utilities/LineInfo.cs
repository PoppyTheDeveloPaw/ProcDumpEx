namespace ProcDumpEx.Utilities;

internal class LineInfo(string FilePath, int LineNumber)
{
	public override string ToString() => $"\"{FilePath}\" (Line: {LineNumber})";
}
