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
		/// Returns the path of the procdump64.exe file if exists, otherwise throws an <see cref="ProcDumpFileMissingException"/>
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ProcDumpFileMissingException"></exception>
		internal static string GetExistingProcDump64Path()
		{
			if (File.Exists(Constants.FullProcdump64FolderPath))
				return Constants.FullProcdump64FolderPath;

			if (File.Exists(Constants.FullProcdump64Path))
				return Constants.FullProcdump64Path;

			throw new ProcDumpFileMissingException(Constants.ProcDump64FileName, Constants.FullProcdump64FolderPath, Constants.FullProcdump64Path);
		}

		/// <summary>
		/// Returns the path of the procdump.exe file if exists, otherwise throws an <see cref="ProcDumpFileMissingException"/>
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ProcDumpFileMissingException"></exception>
		internal static string GetExistingProcDumpPath()
		{
			if (File.Exists(Constants.FullProcdumpFolderPath))
				return Constants.FullProcdumpFolderPath;

			if (File.Exists(Constants.FullProcdumpPath))
				return Constants.FullProcdumpPath;

			throw new ProcDumpFileMissingException(Constants.ProcDumpFileName, Constants.FullProcdumpFolderPath, Constants.FullProcdumpPath);
		}

		/// <summary>
		/// Returns the path of the procdump.exe file if exists, otherwise throws an <see cref="ProcDumpFileMissingException"/>
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ProcDumpFileMissingException"></exception>
		internal static string GetExistingProcDump64aPath()
		{
			if (File.Exists(Constants.FullProcdump64aFolderPath))
				return Constants.FullProcdump64aFolderPath;

			if (File.Exists(Constants.FullProcdump64aPath))
				return Constants.FullProcdump64aPath;

			throw new ProcDumpFileMissingException(Constants.ProcDump64aFileName, Constants.FullProcdump64aFolderPath, Constants.FullProcdump64aPath);
		}

		internal static bool CheckAdministratorPrivileges()
		{
			if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
			{
				ConsoleEx.WriteInfo("Administrator privileges are required to run ProcDumpEx. Please restart the console as administrator.", "Helper");
				return false;
			}
			return true;
		}

		internal static bool IsProcdumpFileMissing(string logId)
		{
			bool procdumpFileMissing = false;

			try
			{
				GetExistingProcDump64Path();
			}
			catch (ProcDumpFileMissingException e)
			{
				ConsoleEx.WriteError(e.Message, logId);
				procdumpFileMissing = true;
			}

			try
			{
				GetExistingProcDumpPath();
			}
			catch (ProcDumpFileMissingException e)
			{
				ConsoleEx.WriteError(e.Message, logId);
				procdumpFileMissing = true;
			}

			try
			{
				GetExistingProcDump64aPath();
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
	}
}
