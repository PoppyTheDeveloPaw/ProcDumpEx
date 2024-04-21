using Microsoft.Win32;
using ProcDumpEx.Exceptions;
using ProcDumpEx.Options;
using System.Diagnostics;
using System.Management;
using System.Reflection;
using System.Security.Principal;

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
		internal static string[] GetDescription(this Type type)
		{
			string filePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), $"Description\\{type.Name}_Description.txt"));

			if (!File.Exists(filePath))
				return new string[] { $"Description file for parameter \"{type.GetOption()}\" not available. Expected under {filePath}" };

			var descContent = File.ReadAllLines(filePath);

			if (descContent.Any(o => !string.IsNullOrEmpty(o)))
				return descContent;

			return new string[] { $"The description file {filePath} exists but is empty" };
		}

		internal static OptionAttribute GetOptionAttribute(this Type type) => (OptionAttribute)type.GetCustomAttribute(typeof(OptionAttribute))!;

		/// <summary>
		/// Checks if the given procdump file exists and returns it in good case, if not an exception is thrown
		/// </summary>
		/// <param name="fileName">Name of the procdump file.</param>
		/// <param name="folderName">Name of the folder in which the procdump file could be located</param>
		/// <returns>Path of the given ProcDump file if it exists</returns>
		/// <exception cref="ProcDumpFileMissingException">Is thrown if file not found.</exception>
		internal static string GetExistingProcDumpPath(ProcDumpVersion procDump)
		{
			if (!Constants.ProcDumpDict.TryGetValue(procDump, out var procDumpInfo))
			{
				// Should never happen
				throw new ArgumentException("Given parameter is unknown", nameof(procDump));
			}

			string relativeFilePath = @$".\{procDumpInfo.FileName}";
			string relativeFolderPath = @$".\{procDumpInfo.FolderName}\{procDumpInfo.FileName}";

			string absoluteFilePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativeFilePath));
			string absoluteFolderPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativeFolderPath));

			if (File.Exists(absoluteFolderPath))
				return absoluteFolderPath;

			if (File.Exists(absoluteFilePath))
				return absoluteFilePath;

			throw new ProcDumpFileMissingException(procDumpInfo.FileName, absoluteFolderPath, absoluteFilePath);
		}

		internal static bool CheckAdministratorPrivileges()
		{
			if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
			{
				ConsoleEx.WriteError("Administrator privileges are required to run ProcDumpEx. Please restart the console as administrator.", "Helper");
				return false;
			}
			return true;
		}

		internal static bool IsProcdumpFileMissing(string logId)
		{
			bool procdumpFileMissing = false;

			try
			{
				GetExistingProcDumpPath(ProcDumpVersion.ProcDump64);
			}
			catch (ProcDumpFileMissingException e)
			{
				ConsoleEx.WriteError(e.Message, logId);
				procdumpFileMissing = true;
			}

			try
			{
				GetExistingProcDumpPath(ProcDumpVersion.ProcDump);
			}
			catch (ProcDumpFileMissingException e)
			{
				ConsoleEx.WriteError(e.Message, logId);
				procdumpFileMissing = true;
			}

			try
			{
				GetExistingProcDumpPath(ProcDumpVersion.ProcDump64a);
			}
			catch (ProcDumpFileMissingException e)
			{
				ConsoleEx.WriteError(e.Message, logId);
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

		internal static bool CheckEula()
		{
			bool eulaAccepted = false;

			if (Registry.GetValue("HKEY_CURRENT_USER\\Software\\Sysinternals\\ProcDump", "EulaAccepted", 0) is int regValue)
				eulaAccepted = regValue == 1;

			if (!eulaAccepted)
			{
				ConsoleEx.WriteInfo("Before you can use ProcDumpEx you must accept the End User License Agreement (EULA) of ProcDump. Do you want to do this now (y/n):", "Helper");

				string? value = Console.ReadLine();

				if (!string.IsNullOrEmpty(value) && value.ToLower() != "y")
				{
					ConsoleEx.WriteError("By entering anything other than \"y\" you have not agreed to ProcDump's End User License Agreement (EULA). ProcDumpEx is terminated.", "Helper");
					return false;
				}

				Registry.SetValue("HKEY_CURRENT_USER\\Software\\Sysinternals\\ProcDump", "EulaAccepted", 1);
			}

			return true;
		}

		internal static void FixArgs(string[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].Contains(','))
				{
					if (!args[i].StartsWith('"'))
						args[i] = $"\"{args[i]}";
					if (!args[i].EndsWith('"'))
						args[i] = $"{args[i]}\"";
				}
			}

			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].Contains(' '))
				{
					if (!args[i].StartsWith('"'))
						args[i] = $"\"{args[i]}";
					if (!args[i].EndsWith('"'))
						args[i] = $"{args[i]}\"";
				}
			}
		}
	}
}
