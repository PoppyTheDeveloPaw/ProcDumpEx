﻿using ProcDumpEx.Exceptions;
using ProcDumpEx.Options;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ProcDumpEx
{
	internal class ProcDumpExCommandParser
	{

		internal static ProcDumpExCommand? Parse(string commandLine, int logId)
		{
			//Split proc dump ex command in different tokens
			var tokens = CommandSplitList.SplitCommandLineString(commandLine);

			ThrowExceptionIfDuplicatedOptionsExists(tokens);

			var types = Helper.GetTypesWithOptionAttribute(Assembly.GetExecutingAssembly());

			(List<string> ProcessNames, List<int> ProcessIds) processes;

			List<OptionBase> options = new List<OptionBase>();

			//If -cfg is passed as argument, the remaining parameters can be ignored
			if (ParseAndRemoveOptionValueIfExists<OptionCfg>(tokens) is { } cfgValue)
			{
				options.Add(cfgValue);
				return new ProcDumpExCommand(options, null!, null!, tokens.ToString(), logId.ToString());
			}

			try
			{
				processes = ExtractProcesses(tokens);
			}
			catch (Exception e) when (e is ArgumentException or ValueExpectedException)
			{
				ConsoleEx.WriteError(e.Message, "ProcDumpExCommandParser");
				return null;
			}

			string pnOption = typeof(OptionPn).GetOption();
			string cfgOption = typeof(OptionCfg).GetOption();

			foreach (var type in types)
			{
				if (type.GetOption() == pnOption || type.GetOption() == cfgOption)
					continue;

				if (ParseAndRemoveOptionValueIfExists(type, tokens) is { } option)
					options.Add(option);
			}

			return new ProcDumpExCommand(options, processes.ProcessNames.Distinct().ToList(), processes.ProcessIds.Distinct().ToList(), tokens.ToString(), logId.ToString());
		}

		private static (List<string> ProcessNames, List<int> ProcessIds) ExtractProcesses(CommandSplitList tokens)
		{
			if (ParseAndRemoveOptionValueIfExists<OptionPn>(tokens) is { } optionPn)
			{
				return ParseProcessStringList(optionPn.Processes.ToArray());
			}
			else
			{
				var obj = tokens.Last();

				if (obj.IsOption)
				{
					OptionAttribute pnOptionAttribute = (OptionAttribute)Attribute.GetCustomAttribute(typeof(OptionPn), typeof(OptionAttribute))!;

					if (obj.Value.ToString()?.ToLower() == typeof(OptionHelp).GetOption())
						return (Array.Empty<string>().ToList(), Array.Empty<int>().ToList());

					throw new ValueExpectedException("Since the parameter {0} was not specified, a list with one or more process names/process id's was expected at the end of the ProcDumpEx command", pnOptionAttribute.Option);
				}

				tokens.RemoveAt(tokens.Count - 1);

				if (obj.Value is List<string> processes)
					return ParseProcessStringList(processes.ToArray());

				return ParseProcessStringList((string)obj.Value);
			}
		}
		private static OptionBase? ParseAndRemoveOptionValueIfExists(Type type, CommandSplitList tokens)
		{
			int index = tokens.GetIndexOfOption(type.GetOption());

			if (index == -1)
				return null;

			(bool IsOption, object Value)? nextToken = index + 1 < tokens.Count ? tokens[index + 1] : null;

			try
			{
				if (!nextToken.HasValue || nextToken.Value.IsOption || !type.GetValueExpected())
				{
					if (type.GetValueExpected())
						throw new ValueExpectedException("For the option {0} one or more values are expected", type.GetOption());

					tokens.RemoveAt(index);

					return (OptionBase)Activator.CreateInstance(type)!;
				}

				tokens.RemoveAt(index + 1);
				tokens.RemoveAt(index);

				if (nextToken.Value.Value is List<string> valueList)
					return (OptionBase)Activator.CreateInstance(type, valueList.ToArray())!;

				return (OptionBase)Activator.CreateInstance(type, (string)nextToken.Value.Value)!;
			}
			finally
			{
				if (type == typeof(OptionPn))
				{
					//If placeholder is not actively added into the arguments by the user, placeholder will be added in place of -pn
					if (!tokens.Any(o => o.IsOption == false && o.Value is string strValue && strValue == Constants.ProcessPlaceholder))
					{
						tokens.Insert(index, (false, Constants.ProcessPlaceholder));
					}
				}
			}
		}

		private static TReturnValue? ParseAndRemoveOptionValueIfExists<TReturnValue>(CommandSplitList tokens) where TReturnValue : OptionBase => (TReturnValue?)ParseAndRemoveOptionValueIfExists(typeof(TReturnValue), tokens);

		private static (List<string> ProcessNames, List<int> ProcessIds) ParseProcessStringList(params string[] processes)
		{
			List<string> processNames = new();
			List<int> processIds = new();

			foreach (var processParam in processes)
			{
				if (int.TryParse(processParam, out int id))
				{
					processIds.Add(id);
					continue;
				}

				if (Regex.IsMatch(processParam, @"[.]*\.exe$"))
				{
					processNames.Add(processParam);
					continue;
				}

				throw new ArgumentException($"No valid process name or process id: {processParam}. Only process IDs (numeric values) or process names (*.exe) are expected.");
			}

			return (processNames, processIds);
		}

		/// <summary>
		/// Throws Exception <see cref="DuplicateOptionsException"/> if an option was used more than once
		/// </summary>
		/// <param name="tokens"></param>
		/// <exception cref="DuplicateOptionsException"></exception>
		private static void ThrowExceptionIfDuplicatedOptionsExists(CommandSplitList tokens)
		{
			var options = tokens.Where(o => o.IsOption).Select(o => o.Value as string);

			var duplicateKeys = options.GroupBy(x => x)
						.Where(group => group.Count() > 1)
						.Select(group => group.Key);

			if (duplicateKeys.Any())
				throw new DuplicateOptionsException(duplicateKeys!); //If the IsOption flag is set, the value is always a string
		}
	}
}
