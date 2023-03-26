using ProcDumpEx.Exceptions;
using ProcDumpEx.Options;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ProcDumpEx
{
	internal class ProcDumpExCommandParser
	{

		internal static ProcDumpExCommand Parse(string commandLine)
		{
			//Split proc dump ex command in different tokens
			var tokens = CommandSplitList.SplitCommandLineString(commandLine);

			ThrowExceptionIfDuplicatedOptionsExists(tokens);

			var types = Helper.GetTypesWithOptionAttribute(Assembly.GetExecutingAssembly());

			var processes = ExtractProcesses(tokens);
			List<OptionBase> options = new List<OptionBase>();

			string pnOption = typeof(OptionPn).GetOption();

			foreach (var type in types)
			{
				if (type.GetOption() == pnOption)
					continue;

				if (ParseAndRemoveOptionValueIfExists(type, tokens) is { } option)
					options.Add(option);
			}

			return new ProcDumpExCommand(options, processes.ProcessNames.Distinct().ToList(), processes.ProcessIds.Distinct().ToList(), tokens.ToString());
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
				tokens.RemoveAt(tokens.Count - 1);

				if (obj.IsOption)
				{
					OptionAttribute pnOptionAttribute = (OptionAttribute)Attribute.GetCustomAttribute(typeof(OptionPn), typeof(OptionAttribute))!;
					throw new ValueExpectedException("Since the parameter {0} was not specified, a list with one or more process names/process id's was expected at the end of the ProcDumpEx command", pnOptionAttribute.Option); //TODO Help fall abdecken
				}

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

			if (!nextToken.HasValue || nextToken.Value.IsOption)
			{
				if (type.GetValueExpected())
					throw new ValueExpectedException("For the option {0} one or more values are expected", type.GetOption());

				tokens.RemoveAt(index);

				return (OptionBase)Activator.CreateInstance(type)!;
			}

			if (!type.GetValueExpected())
				throw new NoValueExpectedException(type.GetOption());

			tokens.RemoveAt(index + 1);
			tokens.RemoveAt(index);

			if (nextToken.Value.Value is List<string> valueList)
				return (OptionBase)Activator.CreateInstance(type, valueList.ToArray())!;

			return (OptionBase)Activator.CreateInstance(type, (string)nextToken.Value.Value)!;
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
