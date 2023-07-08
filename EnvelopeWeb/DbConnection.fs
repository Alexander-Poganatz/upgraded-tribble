module DbConnection

type IDbConnectionGetter =
    abstract member GetNewConnection: unit -> MySqlConnector.MySqlConnection

type DbConnectionGetter(ConnectionString: string) =
    interface IDbConnectionGetter with
        member this.GetNewConnection() = 
            new MySqlConnector.MySqlConnection(ConnectionString=ConnectionString)