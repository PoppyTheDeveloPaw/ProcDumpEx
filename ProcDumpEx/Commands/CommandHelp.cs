
using ProcDumpEx.Exceptions;
using ProcDumpEx.Utilities;
using System.Diagnostics;
using System.Reflection;

namespace ProcDumpEx.Commands;

/// <summary>
/// <inheritdoc cref="IActionCommand" />
/// <para>
/// The -help parameter can be used to display explanations and examples for the individual parameters supported by ProcDumpEx. In addition, the help of ProcDump is printed. 
/// As soon as the parameter '-help' occurs in the specified arguments, nothing is executed but only the help of ProcDumpEx and ProcDump is displayed.
/// </para>
/// </summary>
internal class CommandHelp : IActionCommand
{
	/// <summary>
	/// The constant representing the command name.
	/// </summary>
	public const string CommandName = "-help";

	/// <inheritdoc />
	public string GetCommandName() => CommandName;

	/// <inheritdoc />
	public bool Validate(LineInfo? lineInfo) => true;

	/// <inheritdoc />
	public async Task RunAsync(Executor executor)
	{
		Version? version = Assembly.GetExecutingAssembly().GetName().Version;

		Logger.AddOutput($"ProcDumpEx {(version is not null ? $"v{version}" : string.Empty)} - Extension to Microsoft's Sysinternal Tool ProcDump");
		Logger.AddOutput($"Copyright (C) Alexander Peipp and Daniel Eichner");
		Logger.AddEmptyLine();
		Logger.AddUnderlinedOutput("ProcDumpEx-Help:");
		Logger.AddEmptyLine();
		Logger.AddOutput("ProcDumpEx extends ProcDump with additional functionality, such as process monitoring and/or simplified parameter input.");
		Logger.AddOutput("For a better overview, https://github.com/PoppyTheDeveloPaw/ProcDumpEx can be visited");
		Logger.AddEmptyLine();
		Logger.AddOutput("ProcDumpEx provides the following additional parameters:");
		Logger.AddEmptyLine();

		foreach (var command in CommandDict.CommandTypes.GetAllCommandNames())
		{
			PrintCommandDescription(command);
		}

		//ProcDump help
		Logger.AddEmptyLine();
		Logger.AddOutput("Below is the usage of procdump itself");
		await PrintUsageOfProcDump();
	}

	/// <inheritdoc />
	public Task StopAsync()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandHelp"/> class
	/// </summary>
	public CommandHelp()
	{

	}

	/// <summary>
	/// Outputs the name of the command and the description from the description file.
	/// </summary>
	/// <param name="command">Name of the command</param>
	internal void PrintCommandDescription(string command)
	{
		string filePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), $"Description\\{command}_Description.txt"));

		try
		{
			Logger.AddUnderlinedOutput(command);

			if (!File.Exists(filePath))
			{
				Logger.AddOutput($"Description file for parameter \"{command}\" not available. Expected under {filePath}");
				return;
			}

			var descContent = File.ReadAllLines(filePath);

			if (descContent.Any(o => !string.IsNullOrEmpty(o)))
			{
				foreach (var descLine in descContent)
				{
					Logger.AddOutput(descLine);
				}
				return;
			}

			Logger.AddOutput($"The description file for parameter \"{command}\" exists but is empty. File path: {filePath}");
		}
		finally
		{
			Logger.AddEmptyLine();
		}
	}

	internal async Task PrintUsageOfProcDump()
	{
		var process = new Process();

		try
		{
			process.StartInfo = new ProcessStartInfo(Utils.GetProcdumpPath(ProcdumpVersion.x86), "-?")
			{
				UseShellExecute = false,
				RedirectStandardOutput = true,
				CreateNoWindow = true
			};
		}
		catch (ProcdumpFileMissingException e)
		{
			Logger.AddException(e.Message, e);
			return;
		}

		process.OutputDataReceived += (sender, e) =>
		{
			if (e.Data != null)
			{
				Logger.AddOutput(e.Data);
			}
		};

		process.Start();
		process.BeginOutputReadLine();
		await process.WaitForExitAsync();
	}
}
