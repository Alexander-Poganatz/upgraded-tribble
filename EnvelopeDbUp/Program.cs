

// See https://aka.ms/new-console-template for more information
using DbUp;
using DbUp.Engine;
using EnvelopeDbUp;
using Microsoft.Extensions.Configuration;

int returnCode = 1;

Console.WriteLine("Hello, World!");

string executingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;

var appsettings = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
#if DEBUG
    .AddUserSecrets<Program>()
#endif
    .AddCommandLine(args)
    .Build();

var connectionString = appsettings["dbConnection"];

var iniPath = appsettings["dbINIPath"];
if(string.IsNullOrWhiteSpace(iniPath) is false)
{
    var iniConfig = new ConfigurationBuilder().AddIniFile(iniPath).Build();
    var mariaEnvelopeSection = iniConfig.GetSection(appsettings["dbINISection"] ?? "MariaEnvelope");
    if (mariaEnvelopeSection != null)
    {
        var serverName = mariaEnvelopeSection["Servername"];
        var databaseName = mariaEnvelopeSection["Database"];
        var uid = mariaEnvelopeSection["UID"];
        var pwd = mariaEnvelopeSection["PWD"];
        connectionString = $"server={serverName};uid={uid};pwd={pwd};database={databaseName};";
    }
}

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Error: no connection string is provided");
    return returnCode;
}

var oneTimeOptions = new SqlScriptOptions() { ScriptType = DbUp.Support.ScriptType.RunOnce, RunGroupOrder = 1 };
var alwaysOptions = new SqlScriptOptions() { ScriptType = DbUp.Support.ScriptType.RunAlways, RunGroupOrder = 255 };

var upgrader = DeployChanges.To
    .MySqlDatabase(new CustomMySQLConnector(connectionString))
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