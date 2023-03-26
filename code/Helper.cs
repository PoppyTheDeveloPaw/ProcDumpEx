using ProcDumpEx.Options;
using System.Management;
using System.Reflection;
namespace ProcDumpEx
{
	internal static class Helper
	{
		internal static IEnumerable<Type> GetTypesWithOptionAttribute(Assembly assembly)
		{
			foreach (Type type in assembly.GetTypes())
			{
				if (type.GetCustomAttributes(typeof(OptionAttribute), true).Length > 0)
				{
					yield return type;
				}
			}
		}

		internal static string GetOption(this Type type) => GetOptionAttribute(type).Option;
		internal static bool GetValueExpected(this Type type) => GetOptionAttribute(type).ValueExpected;
		internal static string GetDescription(this Type type)
		{
			string filePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), $"Description\\{type.Name}_Description.txt"));

			if (!File.Exists(filePath))
				return $"Description file for parameter \"{type.GetOption()}\" not available. Expected under {filePath}";

			string descContent = File.ReadAllText(filePath);

			if (!string.IsNullOrEmpty(descContent))
				return descContent;

			return $"The description file {filePath} exists but is empty";
		}

		internal static OptionAttribute GetOptionAttribute(this Type type) => (OptionAttribute)type.GetCustomAttribute(typeof(OptionAttribute))!;

		internal static bool IsProcdumpFileMissing()
		{
			bool procdumpFileMissing = false;
			if (!File.Exists(Constants.FullProcdumpPath))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"The following file is required for execution: {Constants.FullProcdumpPath}");
				procdumpFileMissing = true;
			}

			if (!File.Exists(Constants.FullProcdump64Path))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"The following file is required for execution: {Constants.FullProcdump64Path}");
				procdumpFileMissing = true;
			}

			return procdumpFileMissing;
		}

		internal static bool TryGetValue<TValueType>(this PropertyDataCollection dataCollection, string propertyName, out TValueType? value)
		{
			value = default;
			foreach (var item in dataCollection)
			{
				if (item.Name == propertyName)
				{
					if (Convert.ChangeType(item.Value.ToString(), typeof(TValueType)) is { } v)
					{
						value = (TValueType)v;
						return true;
					}
					break;
				}
			}
			return false;
		}
	}
}
