﻿namespace ProcDumpEx.Options
{
	[Option("-ta", true, "No value provided for -ta. Please provide a value in the format 999d23h59m59s.")]
	internal class OptionTa : OptionBase
	{
		internal override bool IsCommandCreator => false;
		public TimeSpan TerminationTimeSpan { get; }

		public OptionTa(string timeString)
		{
			TerminationTimeSpan = ParseStringToTimeSpan(timeString);

			if (TerminationTimeSpan.TotalMilliseconds <= 0)
			{
				throw new ArgumentException($"{GetType().GetOption()} expects an input that is greater than 0 days, 0 hours, 0 minutes and 0 seconds");
			}

			// TerminationTimeSpan + UtcNow must not be greater than 9999 years, as this is the maximum of DateTime
			if (TerminationTimeSpan.TotalDays > (DateTime.MaxValue - DateTime.UtcNow).TotalDays)
			{
				throw new ArgumentException($"{GetType().GetOption()} expects an input value which is selected in such a way that if " +
					$"you convert the value into years and add it to the current year, the result is a number <= 9999.");
			}
		}

		internal override Task<bool> ExecuteAsync(ProcDumpExCommand command)
		{
			throw new NotSupportedException();
		}

		private TimeSpan ParseStringToTimeSpan(string time)
		{
			time = time.Trim();

			if (string.IsNullOrWhiteSpace(time))
			{
				throw new ArgumentException($"{GetType().GetOption()} expects at least one of the parameters 'd', 'h', 'm' or 's' to be defined.");
			}

			if (time.Any(o => char.IsAsciiLetter(o) && o is not ('d' or 'h' or 'm' or 's' or 'D' or 'H' or 'M' or 'S')))
			{
				throw new ArgumentException($"{GetType().GetOption()} expects a time span string as a parameter, which may only contain the letters 'd', 'h', 'm' and/or 's' and the corresponding times.");
			}

			if (!time.Any(char.IsAsciiLetter))
			{
				throw new ArgumentException($"{GetType().GetOption()} expects at least one of the parameters 'd', 'h', 'm' or 's' to be defined.");
			}

			int days = ExtractNumberFromString(time, 'd') ?? 0;
			if (days < 0)
			{
				throw new ArgumentException($"{GetType().GetOption()} expects only positive numbers not greater than 10675199 for the number before the letter 'd'");
			}

			int hours = ExtractNumberFromString(time, 'h') ?? 0;
			if (hours is (> 23 or < 0))
			{
				throw new ArgumentException($"{GetType().GetOption()} expects only numbers between 0 and 23 for the number before the letter 'h'");
			}

			int minutes = ExtractNumberFromString(time, 'm') ?? 0;
			if (minutes is (> 59 or < 0))
			{
				throw new ArgumentException($"{GetType().GetOption()} expects only numbers between 0 and 59 for the number before the letter 'm'");
			}

			int seconds = ExtractNumberFromString(time, 's') ?? 0;
			if (seconds is (> 59 or < 0))
			{
				throw new ArgumentException($"{GetType().GetOption()} expects only numbers between 0 and 59 for the number before the letter 's'");
			}

			try
			{
				return new TimeSpan(days, hours, minutes, seconds);
			}
			catch
			{
				throw new ArgumentException($"{GetType().GetOption()} expects a time span not greater than {Helper.GetFormattedTimeSpanString(TimeSpan.MaxValue)}.");
			}
			
		}

		/// <summary>
		/// Extracts the number for the given <paramref name="timeChar"/> from the <paramref name="time"/>
		/// </summary>
		/// <param name="time">Time string from which the number is to be extracted</param>
		/// <param name="timeChar">Char that identifies the number in the string</param>
		/// <returns>Returns the number of the char if it occurs, otherwise <see langword="null"/></returns>
		private static int? ExtractNumberFromString(string time, char timeChar)
		{
			int index = time.IndexOf(timeChar, StringComparison.OrdinalIgnoreCase);

			if (index == -1)
			{
				// Time for the given char not specified => default: null
				return null;
			}

			List<char> numericChars = [];
			for (int i = index - 1; i >= 0; i--)
			{
				if (!char.IsDigit(time[i]) && time[i] is not '-')
				{
					// As soon as no more numeric characters are found, abort
					break;
				}

				numericChars.Add(time[i]);
			}

			if (numericChars.Count == 0)
			{
				return null;
			}

			numericChars.Reverse();

			string number = string.Join("", numericChars);

			if (int.TryParse(number, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.CurrentCulture, out int result))
			{
				return result;
			}

			return -1;
		}
	}
}
