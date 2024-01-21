namespace ProcDumpEx.Exceptions;

/// <summary>
/// Is thrown if an opening quote was found when parsing the input parameters, but no closing quote was found.
/// </summary>
internal class EndQuoteMissingException : Exception
{
}
