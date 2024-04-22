namespace ProcDumpEx;

internal static class Program
{
	private static async Task Main(string[] args)
	{
		if(!Helper.CheckAdministratorPrivileges())
			return;

		if(!Helper.CheckEula())
			return;

		if(Helper.IsProcDumpFileMissing("Base"))
			return;

		Helper.FixArgs(args);

		if(ArgumentManager.GetCommands(args) is not { } commandList)
			return;

		ProcDumpEx procDumpEx = new(commandList);
		await procDumpEx.RunAsync();
	}
}
