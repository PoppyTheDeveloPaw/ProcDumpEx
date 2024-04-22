using System.Text;

namespace ProcDumpEx
{
	internal class CommandSplitList : List<(bool IsOption, object Value)>
	{
		/// <summary>
		/// Searches for the specified ProcDumpExOption and returns the zero-based index of the first occurrence within
		/// the range of elements in the CommandSplitList that extends from the specified index to the last element.
		/// </summary>
		/// <param name="option"></param>
		/// <returns>
		/// The zero-based index of the first occurrence of item within the range of elements in the CommandSplitList
		/// that extends from index to the last element, if found; otherwise, -1.
		/// </returns>
		internal int GetIndexOfOption(string option)
		{
			for (int i = 0; i < Count; i++)
			{
				if (!this[i].IsOption)
				{
					continue;
				}

				//Values with the "IsOption" flag set are always of type string
				if (this[i].Value.ToString() is not { } strValue)
				{
					continue;
				}

				if (strValue.Equals(option, StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Splits the given command line string into options and values.
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>
		internal static CommandSplitList SplitCommandLineString(string commandLine)
		{
			CommandSplitList retTokens = [];

			var tokens = commandLine.Split(' ');

			int index = 0;
			while (index < tokens.Length)
			{
				string token = tokens[index];

				if (string.IsNullOrEmpty(token))
				{
					index++;
					continue;
				}

				if (token.StartsWith('-'))
				{
					retTokens.Add((IsOption: true, Value: token));
					index++;
					continue;
				}

				if (!token.StartsWith('"'))
				{
					retTokens.Add((IsOption: false, Value: token));
					index++;
					continue;
				}

				//Goes through the list until it finds '"' at the end of a value
				int x = index;
				for (; x < tokens.Length; x++)
					if (tokens[x].EndsWith('"'))
						break;

				//Merges all values inside the quotes again and splits them with comma
				string value = string.Join(" ", tokens[index..(x + 1)]);
				retTokens.Add((IsOption: false, Value: value.Trim('"').Split(',').Select(o => o.Trim()).ToList()));

				index = x;
			}

			return retTokens;
		}

		public override string ToString()
		{
			StringBuilder sb = new();

			foreach (var value in this.Select(o => o.Value))
			{
				if (value is List<string> values)
				{
					sb.Append('"');
					sb.Append(string.Join(',', values));
					sb.Append('"');
				}
				else
				{
					sb.Append(value.ToString());
				}
				sb.Append(' ');
			}

			return sb.ToString();
		}
	}
}
