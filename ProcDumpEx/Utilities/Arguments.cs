using ProcDumpEx.Commands;
using System.Diagnostics.CodeAnalysis;

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

	/// <summary>
	/// Initializes a new instance of the <see cref="Argument"/> class
	/// </summary>
	/// <param name="command">Command name</param>
	/// <param name="arguments">Command arguments</param>
	public Argument(string command, List<string> arguments)
	{
		Command = command;
		Arguments = arguments;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Argument"/> class
	/// </summary>
	/// <param name="command">Command name</param>
	public Argument(string command)
	{
		Command = command;
	}

	/// <summary>
	/// Try parsing the <see cref="Argument"/> object into an <see cref="ICommand"/> object.
	/// </summary>
	/// <param name="command">On return, contains the result of successfully parsing or null on failure.</param>
	/// <returns><see langword="True"/> if parsing was successful; otherwise <see langword="false"/></returns>
	public bool TryParse([NotNullWhen(true)] out ICommand? command )
	{
		try
		{
			if (Arguments is null || !Arguments.Any())
			{
				command = CommandDict.CommandTypes.CreateObject(Command);
				return command != null;
			}
			else
			{
				command = CommandDict.CommandTypes.CreateObject(Command, Arguments.ToArray());
			}
			return command != null;
		}
		catch (MissingMethodException)
		{
			command = null;
			return false;
		}
	}
}
