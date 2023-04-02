﻿using ProcDumpEx;
using ProcDumpEx.code;
using System.Drawing;

if (Helper.IsProcdumpFileMissing())
	return;

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

if (ArgumentManager.GetCommands(args) is not { } commandList)
	return;

KeyEvent.Instance.KeyPressedEvent += (sender, e) => Instance_KeyPressedEvent(e, commandList);
AppDomain.CurrentDomain.ProcessExit += (sender, e) => ProcessExitEvent(commandList);

bool manuallyExit = false;
string text = string.Empty;

List<Task> awaitCommands = new List<Task>();
foreach (var command in commandList)
{
	awaitCommands.Add(command.RunAsync());
}
await Task.WhenAll(awaitCommands);

if (manuallyExit)
	ConsoleEx.WriteColor(text, ConsoleColor.DarkMagenta);

if (commandList.Any(o => o.Log))
	ConsoleEx.WriteLogFile();

void Instance_KeyPressedEvent(KeyPressed e, ProcDumpExCommand[] commands)
{
	manuallyExit = true;
	string key = e switch
	{
		KeyPressed.Ctrl_Break => "CTRL + Break",
		KeyPressed.Ctrl_C => "CTRL + C",
		KeyPressed.X => "X",
		_ => "Unknown"
	};
	text = $"ProcDump was terminated manually by pressing the \"{key}\" key";

	foreach (var command in commands)
	{
		command.Stop();
	}
}

void ProcessExitEvent(ProcDumpExCommand[] commands)
{
	foreach (var command in commands)
	{
		command.Stop();
	}
}