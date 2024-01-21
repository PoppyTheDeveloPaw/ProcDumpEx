using ProcDumpEx.Commands;

namespace ProcDumpEx;

internal class ParseResult
{
	/// <summary>
	/// Arguments that are used when executing ProcDump
	/// </summary>
	internal readonly string BaseProcdumpCommand;

	/// <summary>
	/// Arguments that are not part of ProcDump, but are added by this extension
	/// </summary>
	internal readonly IReadOnlyList<ICommand> Commands;


	internal readonly (IReadOnlyList<string> ProcessNames, IReadOnlyList<int> ProcessIds) Processes;

	public ParseResult(IReadOnlyList<ICommand> commands, (IReadOnlyList<string>, IReadOnlyList<int>) processes, string baseProcdumpCommand)
	{
		Commands = commands;
		Processes = processes;
		BaseProcdumpCommand = baseProcdumpCommand;
	}
}
