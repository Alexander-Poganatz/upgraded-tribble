using DbUp.Engine.Transactions;
using DbUp.MySql;

namespace EnvelopeDbUp
{
    /// <summary>
    /// Aight, so MySQL and MariaDB are diverging
    /// MariaDB seems to have put Null in some system table when MySql driver checks for character sets so the MySql driver breaks
    /// It looks like the DbUp MySqlConnector class does't have much and I can just inject the MySqlConnector package DbConnection implementation
    /// in and use the work done in DbUp to handle whatever SplitScriptIntoCommands does
    /// </summary>
    internal class CustomMySQLConnector : DatabaseConnectionManager
    {

        public CustomMySQLConnector(string connectionString) : base(new DelegateConnectionFactory(l => new MySqlConnector.MySqlConnection(connectionString))) { }

        public override IEnumerable<string> SplitScriptIntoCommands(string scriptContents)
        {
            var mySQLConnectionManager = new MySqlConnectionManager(string.Empty);
            return mySQLConnectionManager.SplitScriptIntoCommands(scriptContents);
        }
    }
}
