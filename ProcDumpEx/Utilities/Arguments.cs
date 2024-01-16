namespace ProcDumpEx.Utilities;

internal class Argument
{
	/// <summary>
	/// Command name
	/// </summary>
	internal string Command { get; }

	/// <summary>
	/// Arguments for the command.
	/// </summary>
	internal IReadOnlyList<string>? Arguments { get; }

	public Argument(string command, List<string> arguments)
	{
		Command = command;
		Arguments = arguments;
	}

	public Argument(string command)
	{
		Command = command;
	}
}
