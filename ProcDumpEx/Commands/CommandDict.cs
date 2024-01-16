using ProcDumpEx.Utilities;

namespace ProcDumpEx.Commands;

internal static class CommandDict
{
	private static TypeDictionary<ICommand> _commandTypes = new TypeDictionary<ICommand>();
	
	internal static TypeDictionary<ICommand> CommandTypes
	{
		get
		{
			if (_commandTypes.IsEmpty())
			{
				Initialize();
			}

			return _commandTypes;
		}
	}

	private static void Initialize()
	{
		_commandTypes.AddType(Command64.CommandName, typeof(Command64));
		_commandTypes.AddType(CommandCfg.CommandName, typeof(CommandCfg));
		_commandTypes.AddType(CommandCputhd.CommandName, typeof(CommandCputhd));
		_commandTypes.AddType(CommandCputhdl.CommandName, typeof(CommandCputhdl));
		_commandTypes.AddType(CommandHelp.CommandName, typeof(CommandHelp));
		_commandTypes.AddType(CommandInf.CommandName, typeof(CommandInf));
		_commandTypes.AddType(CommandLog.CommandName, typeof(CommandLog));
		_commandTypes.AddType(CommandMemthd.CommandName, typeof(CommandMemthd));
		_commandTypes.AddType(CommandMemthdl.CommandName, typeof(CommandMemthdl));
		_commandTypes.AddType(CommandPn.CommandName, typeof(CommandPn));
		_commandTypes.AddType(CommandShowOutput.CommandName, typeof(CommandShowOutput));
		_commandTypes.AddType(CommandW.CommandName, typeof(CommandW));
	}
}
