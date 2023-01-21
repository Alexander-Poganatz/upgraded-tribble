

// See https://aka.ms/new-console-template for more information
using DbUp;
using DbUp.Engine;
using Microsoft.Extensions.Configuration;

int returnCode = 1;

Console.WriteLine("Hello, World!");

string executingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;

var appsettings = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddCommandLine(args)
    .Build();

var oneTimeOptions = new SqlScriptOptions() { ScriptType = DbUp.Support.ScriptType.RunOnce, RunGroupOrder = 1 };
var alwaysOptions = new SqlScriptOptions() { ScriptType = DbUp.Support.ScriptType.RunAlways, RunGroupOrder = 255 };

var upgrader = DeployChanges.To
    .MySqlDatabase(string.Empty) // todo connection string
    .WithScriptsFromFileSystem(Path.Combine(executingPath, "Scripts", "OneTime"), oneTimeOptions)
    .WithScriptsFromFileSystem(Path.Combine(executingPath, "Scripts", "Always"), alwaysOptions)
    .WithExecutionTimeout(TimeSpan.FromMinutes(2))
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();



if (!result.Successful)
{
    Console.Error.WriteLine(result.Error);
#if DEBUG
    Console.ReadLine();
#endif
}
else
{
    returnCode = 0;
}

return returnCode;