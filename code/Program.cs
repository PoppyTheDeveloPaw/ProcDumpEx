// See https://aka.ms/new-console-template for more information
using ProcDumpEx;
using ProcDumpEx.code;

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

await command.RunAsync();

void Instance_KeyPressedEvent(KeyPressed e, ProcDumpExCommand command)
{
	command.Stop();
}

void ProcessExitEvent(ProcDumpExCommand command)
{
	command.Stop();
}