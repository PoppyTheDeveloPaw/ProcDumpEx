using ProcDumpEx.Exceptions;
using ProcDumpEx.Options;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ProcDumpEx
{
	internal static partial class ProcDumpExCommandParser
	{
		private static readonly string _pnOption = typeof(OptionPn).GetOption();
		private static readonly string _cfgOption = typeof(OptionCfg).GetOption();
		private static readonly string _helpOption = typeof(OptionHelp).GetOption();

		[GeneratedRegex(@"[.]*\.exe$")]
		private static partial Regex ProcessNameRegex();

		/// <summary>
		/// Parses the given commandline to an <see cref="ProcDumpExCommand"/> object.
		/// </summary>
		/// <param name="commandLine">Command line to parse</param>
		/// <param name="logId">Caller id for logging.</param>
		/// <returns>Returns the <see cref="ProcDumpExCommand"/> object if the parsing was successful; otherwise <see langword="null"/>.</returns>
		internal static ProcDumpExCommand? Parse(string commandLine, int logId)
		{
			//Split proc dump ex command in different tokens
			var tokens = CommandSplitList.SplitCommandLineString(commandLine);

			ThrowExceptionIfDuplicatedOptionsExists(tokens);

			var types = Helper.GetTypesWithOptionAttribute(Assembly.GetExecutingAssembly());

			(List<string> ProcessNames, List<int> ProcessIds) processes;

			List<OptionBase> options = [];

			//If -cfg is passed as argument, the remaining parameters can be ignored
			if (ParseAndRemoveOptionValueIfExists<OptionCfg>(tokens) is { } cfgValue)
			{
				options.Add(cfgValue);
				return new ProcDumpExCommand(options, [], [], tokens.ToString(), logId.ToString());
			}

			try
			{
				processes = ExtractProcesses(tokens);
			}
			catch (Exception e) when (e is ArgumentException or ValueExpectedException)
			{
				ConsoleEx.WriteLog(e.Message, "ProcDumpExCommandParser", LogType.Error);
				return null;
			}

			foreach (var type in types)
			{
				if (type.GetOption() == _pnOption || type.GetOption() == _cfgOption)
					continue;

				if (ParseAndRemoveOptionValueIfExists(type, tokens) is { } option)
					options.Add(option);
			}

			return new ProcDumpExCommand(options, processes.ProcessNames.Distinct().ToList(), processes.ProcessIds.Distinct().ToList(), tokens.ToString(), logId.ToString());
		}

		/// <summary>
		/// Extracts the process names and process IDs from the command tokens
		/// </summary>
		/// <param name="tokens">Given tokens from input command.</param>
		/// <returns>Returns a tuple of ProcessNames and ProcessIds if parsing was succesfull; otherwise an exception is thrown.</returns>
		/// <exception cref="ValueExpectedException">Is thrown when an command parameter was not specified.</exception>
		/// <exception cref="ArgumentException">Is thrown if no valid process names or process ids exists.</exception>
		private static (List<string> ProcessNames, List<int> ProcessIds) ExtractProcesses(CommandSplitList tokens)
		{
			if (ParseAndRemoveOptionValueIfExists<OptionPn>(tokens) is { } optionPn)
			{
				return ParseProcessStringList([.. optionPn.Processes]);
			}
			else
			{
				var (IsOption, Value) = tokens[^1];

				if (IsOption)
				{
					OptionAttribute pnOptionAttribute = (OptionAttribute)Attribute.GetCustomAttribute(typeof(OptionPn), typeof(OptionAttribute))!;

					if (Value.ToString()?.ToLower() == _helpOption)
						return (Array.Empty<string>().ToList(), Array.Empty<int>().ToList());

					throw new ValueExpectedException("Since the parameter {0} was not specified, a list with one or more process names/process id's was expected at the end of the ProcDumpEx command", pnOptionAttribute.Option);
				}

				tokens.RemoveAt(tokens.Count - 1);

				if (Value is List<string> processes)
				{
					return ParseProcessStringList([.. processes]);
				}

				return ParseProcessStringList((string)Value);
			}
		}

		/// <summary>
		/// Parses and removes the option value from the tokens list
		/// </summary>
		/// <param name="type">Command type to be parsed.</param>
		/// <param name="tokens">Tokens for parsing</param>
		/// <returns>An object of the given command type <paramref name="type"/> if parsing was successfull; otherwise <see langword="null"/></returns>
		/// <exception cref="ValueExpectedException">Is thrown if an command value is missing.</exception>
		private static OptionBase? ParseAndRemoveOptionValueIfExists(Type type, CommandSplitList tokens)
		{
			int index = tokens.GetIndexOfOption(type.GetOption());

			if (index == -1)
			{
				return null;
			}

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
				{
					return (OptionBase)Activator.CreateInstance(type, valueList.ToArray())!;
				}

				return (OptionBase)Activator.CreateInstance(type, (string)nextToken.Value.Value)!;
			}
			finally
			{
				if (type == typeof(OptionPn) && !tokens.Exists(o => !o.IsOption && o.Value is string strValue && strValue == Constants.ProcessPlaceholder))
				{
					tokens.Insert(index, (false, Constants.ProcessPlaceholder));
				}
			}
		}

		/// <summary>
		/// Parses and removes the option value from the tokens list
		/// </summary>
		/// <param name="type">Command type to be parsed.</param>
		/// <param name="tokens">Tokens for parsing</param>
		/// <returns>An object of the given command type <paramref name="type"/> if parsing was successfull; otherwise <see langword="null"/></returns>
		/// <exception cref="ValueExpectedException">Is thrown if an command value is missing.</exception>
		private static TReturnValue? ParseAndRemoveOptionValueIfExists<TReturnValue>(CommandSplitList tokens) where TReturnValue : OptionBase => (TReturnValue?)ParseAndRemoveOptionValueIfExists(typeof(TReturnValue), tokens);

		/// <summary>
		/// Parses the process string list and separates process names and process IDs
		/// </summary>
		/// <param name="processes">Process strings to be parsed.</param>
		/// <returns>A pair of process names and ids.</returns>
		/// <exception cref="ArgumentException">Thrown if no valid process names or ids exists.</exception>
		private static (List<string> ProcessNames, List<int> ProcessIds) ParseProcessStringList(params string[] processes)
		{
			List<string> processNames = [];
			List<int> processIds = [];

			foreach (var processParam in processes)
			{
				if (int.TryParse(processParam, out int id))
				{
					processIds.Add(id);
					continue;
				}

				if (ProcessNameRegex().IsMatch(processParam))
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
