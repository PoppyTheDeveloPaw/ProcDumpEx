namespace ProcDumpEx
{
	internal class CommandSplitList : List<(bool IsOption, object Value)>
	{
		/// <summary>
		/// Searches for the specified ProcDumpExOption and returns the zero-based index of the first occurrence within the range of elements in the CommandSplitList that extends from the specified index to the last element.
		/// </summary>
		/// <param name="option"></param>
		/// <returns>The zero-based index of the first occurrence of item within the range of elements in the CommandSplitList that extends from index to the last element, if found; otherwise, -1.</returns>
		internal int GetIndexOfOption(string option)
		{
			for (int i = 0; i < Count; i++ )
			{
				if (!this[i].IsOption)
					continue;

				//Values with the "IsOption" flag set are always of type string
				if (this[i].Value.ToString()!.ToLower() == option)
					return i;
			}

			return -1;
		}

		internal static CommandSplitList SplitCommandLineString(string commandLine)
		{
			CommandSplitList retTokens = new();

			var tokens = commandLine.Split(' ');
			for (int i = 0; i < tokens.Length; i++)
			{
				string token = tokens[i];

				if (string.IsNullOrEmpty(token))
					continue;

				if (token.StartsWith('-'))
				{
					retTokens.Add((IsOption: true, Value: token));
					continue;
				}

				if (!token.StartsWith('"'))
				{
					retTokens.Add((IsOption: false, Value: token));
					continue;
				}

				//Goes through the list until it finds '"' at the end of a value
				int x = i;
				for (; x < tokens.Length; x++)
					if (tokens[x].EndsWith('"'))
						break;

				//Merges all values inside the quotes again and splits them with comma
				string value = string.Join(" ", tokens[i..(x + 1)]);
				retTokens.Add((IsOption: false, Value: value.Trim('"').Split(',').Select(o => o.Trim()).ToList()));

				i = x;
			}

			return retTokens;
		}

		public override string ToString()
		{
			List<string> ret = new();


			foreach (var value in this.Select(o => o.Value))
			{
				if (value is List<string> values)
					ret.Add($"\"{string.Join(',', values)}\"");
				else
					ret.Add(value.ToString()!);
			}

			return string.Join(' ', ret);
		}
	}
}
