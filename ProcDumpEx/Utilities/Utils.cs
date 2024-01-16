using Microsoft.Win32;
using ProcDumpEx.Exceptions;
using System.Security.Principal;

namespace ProcDumpEx.Utilities;

internal static class Utils
{
	/// <summary>
	/// Checks whether the necessary preconditions are met before the program starts properly.
	/// </summary>
	/// <returns><see langword="True"/> if the preconditions are met; otherwise <see langword="false"/>.</returns>
	internal static bool CheckPreconditions()
	{
		try
		{
			CheckAdministratorPrivileges();
			CheckEulaIsAccepted();
			CheckProcdumpFiles();
		}
		catch (Exception e) when (e is AdministratorPrivilegesMissingException or EulaNotAcceptedException or ProcdumpFileMissingException)
		{
			return false;
		}

		return true;
	}

	/// <summary>
	/// Checks whether the program has administrator rights.
	/// </summary>
	/// <exception cref="AdministratorPrivilegesMissingException">Is thrown if the program was started without administrator rights.</exception>
	private static void CheckAdministratorPrivileges()
	{
		var windowsPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
		if (!windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
		{
			Logger.AddOutput("Administrator privileges are required to run ProcDumpEx. Please restart the console as administrator.", logType: LogType.Error);
			throw new AdministratorPrivilegesMissingException();
		}
	}

	/// <summary>
	/// Checks whether the Procdump EULA has been accepted, if not there is the option to accept the EULA.
	/// </summary>
	/// <exception cref="EulaNotAcceptedException">Thrown if the EULA is/will not be accepted.</exception>
	private static void CheckEulaIsAccepted()
	{
		bool eulaAccepted = false;

		if (Registry.GetValue("HKEY_CURRENT_USER\\Software\\Sysinternals\\ProcDump", "EulaAccepted", 0) is int regValue)
			eulaAccepted = regValue == 1;

		if (!eulaAccepted)
		{
			Logger.AddOutput("Before you can use ProcDumpEx you must accept the End User License Agreement (EULA) of ProcDump. Do you want to do this now (y/n):", logType: LogType.Info);

			string? value = Console.ReadLine();

			if (!string.IsNullOrEmpty(value) && value.ToLower() != "y")
			{
				Logger.AddOutput("By entering anything other than \"y\" you have not agreed to ProcDump's End User License Agreement (EULA). ProcDumpEx is terminated.", logType: LogType.Error);
				throw new EulaNotAcceptedException();
			}

			Registry.SetValue("HKEY_CURRENT_USER\\Software\\Sysinternals\\ProcDump", "EulaAccepted", 1);
		}
	}

	/// <summary>
	/// Checks whether the different versions of Procdump are available under the expected path.
	/// </summary>
	/// <exception cref="ProcdumpFileMissingException">Is thrown if procdump files are missing.</exception>
	private static void CheckProcdumpFiles()
	{
		bool allProcdumpFilesExists = true;

		foreach (var e in Enum.GetValues<ProcdumpVersion>())
		{
			try
			{
				GetProcdumpPath(e);
			}
			catch (ProcdumpFileMissingException)
			{
				allProcdumpFilesExists = false;
			}
		}

		if (!allProcdumpFilesExists)
		{
			throw new ProcdumpFileMissingException();
		}
	}

	/// <summary>
	/// Returns the path under which the given Procdummp version is located.
	/// </summary>
	/// <param name="version">Required Procdump version.</param>
	/// <returns>Procdump path if exists; otherwise a <see cref="ProcdumpFileMissingException"/> is thrown.</returns>
	/// <exception cref="ProcdumpFileMissingException">Is thrown if the procdump file is missing.</exception>
	internal static string GetProcdumpPath(ProcdumpVersion version)
	{
#pragma warning disable CS8524
		var paths = version switch
		{
			ProcdumpVersion.x64 => Constants.AbsolutePath_Procdump64,
			ProcdumpVersion.x64a => Constants.AbsolutePath_Procdump64a,
			ProcdumpVersion.x86 => Constants.AbsolutePath_Procump
		};
#pragma warning restore CS8524

		foreach (var path in paths)
		{
			if (File.Exists(path))
			{
				return path;
			}
		}

		Logger.AddOutput($"Procdump file (Version: ${Enum.GetName(version)}) missing. Expected under: {string.Join(" or ", paths)}", logType: LogType.Error);
		throw new ProcdumpFileMissingException();
	}
}
