module DbConnection

type DbConnectionGetter(ConnectionString: string) =
    member this.GetNewConnection() = 
        new System.Data.Odbc.OdbcConnection(ConnectionString=ConnectionString)