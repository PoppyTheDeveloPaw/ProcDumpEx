using ProcDumpEx;
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

string argsCommandLine = string.Join(' ', args);

if (ProcDumpExCommandParser.Parse(argsCommandLine) is not { } command)
{
	ConsoleEx.WriteError("Specified parameters could not be parsed. ProcDumpEx is terminated. Use the parameter \"-help\" to display examples and allowed parameters");
	return;
}

KeyEvent.Instance.KeyPressedEvent += (sender, e) => Instance_KeyPressedEvent(e, command);
AppDomain.CurrentDomain.ProcessExit += (sender, e) => ProcessExitEvent(command);

bool manuallyExit = false;
string text = string.Empty;

await command.RunAsync();

if (manuallyExit)
	ConsoleEx.WriteColor(text, ConsoleColor.DarkMagenta);

if (command.Log)
	ConsoleEx.WriteLogFile();

void Instance_KeyPressedEvent(KeyPressed e, ProcDumpExCommand command)
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
	command.Stop();
}

void ProcessExitEvent(ProcDumpExCommand command)
{
	command.Stop();
}