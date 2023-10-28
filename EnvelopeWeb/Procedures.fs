module Procedures
open System.Data.Odbc
open Models

let private AddParamToCommand (command:OdbcCommand) (paramName:string) (value:obj) =
    let param = OdbcParameter(paramName, value)
    command.Parameters.Add param

let private AddParamWithTypeToCommand (command:OdbcCommand) (paramName:string) (value:obj) dataType =
    let param = OdbcParameter(paramName, value)
    param.OdbcType <- dataType
    command.Parameters.Add param

let InsertUser (dbConnectionGetter:DbConnection.DbConnectionGetter) email passwordHash (passwordConfig:PasswordConfig) =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "ins_User"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamToCommand command "e" email |> ignore
        AddParamWithTypeToCommand command "p" passwordHash OdbcType.Binary |> ignore
        AddParamWithTypeToCommand command "s" passwordConfig.Salt OdbcType.Binary |> ignore
        AddParamWithTypeToCommand command "m" passwordConfig.MiB OdbcType.TinyInt |> ignore
        AddParamWithTypeToCommand command "i" passwordConfig.Iterations OdbcType.TinyInt |> ignore
        AddParamWithTypeToCommand command "dop" passwordConfig.DegreeOfParallism OdbcType.TinyInt |> ignore

    
        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }
    
let UpdateUserPassword (dbConnectionGetter:DbConnection.DbConnectionGetter) uid passwordHash (passwordConfig:PasswordConfig) =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "upd_User_Password"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uid" uid OdbcType.Int |> ignore
        AddParamWithTypeToCommand command "p" passwordHash OdbcType.Binary |> ignore
        AddParamWithTypeToCommand command "s" passwordConfig.Salt OdbcType.Binary |> ignore
        AddParamWithTypeToCommand command "m" passwordConfig.MiB OdbcType.TinyInt |> ignore
        AddParamWithTypeToCommand command "i" passwordConfig.Iterations OdbcType.TinyInt |> ignore
        AddParamWithTypeToCommand command "dop" passwordConfig.DegreeOfParallism OdbcType.TinyInt |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }

let Sel_UserByEmail (dbConnectionGetter:DbConnection.DbConnectionGetter) email =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "sel_UserByEmail"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamToCommand command "e" email |> ignore

    
        do! connection.OpenAsync()

        use! reader' = command.ExecuteReaderAsync()
        let reader = reader' :?> OdbcDataReader

        let! userExists = reader.ReadAsync()

        let user = 
            if userExists then
                let passwordHash = Array.create Password.HashOutputSize 0uy
                let salt = Array.create Password.SaltSize 0uy
                reader.GetBytes(1, 0, passwordHash, 0, passwordHash.Length) |> ignore
                reader.GetBytes(2, 0, salt, 0, salt.Length) |> ignore
                let passwordConfig = { MiB = reader.GetByte(3); Iterations = reader.GetByte(4); DegreeOfParallism = reader.GetByte(5); Salt = salt; }
                let user = { Id = reader.GetInt32(0) |> uint32; PasswordHash = passwordHash; LockoutExpiry = reader.GetDateTime(6); PasswordConfig = passwordConfig }
                Some user
            else
                None

        do! reader.CloseAsync()
        do! connection.CloseAsync()

        return user
    }

let Upd_User_Login (dbConnectionGetter:DbConnection.DbConnectionGetter) uid isValid =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "upd_User_Login"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid OdbcType.Int |> ignore
        AddParamWithTypeToCommand command "isValid" isValid OdbcType.Bit |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }

let Ins_Envelope (dbConnectionGetter:DbConnection.DbConnectionGetter) uid eName =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "ins_Envelope"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamToCommand command "uID" uid |> ignore
        AddParamToCommand command "eName" eName |> ignore

    
        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteScalarAsync()

        do! connection.CloseAsync()

        return rowsInserted |> System.Convert.ToUInt16
    }

let Upd_Envelope (dbConnectionGetter:DbConnection.DbConnectionGetter) uid eNumber newName =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "upd_Envelope"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid OdbcType.Int |> ignore
        AddParamWithTypeToCommand command "eNumber" eNumber OdbcType.SmallInt |> ignore
        AddParamToCommand command "eName" newName |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }

let Del_Envelope (dbConnectionGetter:DbConnection.DbConnectionGetter) uid eNumber =
    task {
    
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "del_Envelope"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid OdbcType.Int |> ignore
        AddParamWithTypeToCommand command "eNumber" eNumber OdbcType.SmallInt |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }

let Ins_EnvelopeTransaction (dbConnectionGetter:DbConnection.DbConnectionGetter) uid eNumber (amount:int) (date:System.DateTime) note =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "ins_EnvelopeTransaction"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid OdbcType.Int |> ignore
        AddParamWithTypeToCommand command "eNumber" eNumber OdbcType.SmallInt |> ignore
        AddParamWithTypeToCommand command "amount" amount OdbcType.Int |> ignore
        AddParamWithTypeToCommand command "tDate" date OdbcType.DateTime |> ignore
        AddParamToCommand command "tNote" note |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync() |> Async.AwaitTask

        do! connection.CloseAsync() |> Async.AwaitTask

        return rowsInserted
    }

let Upd_EnvelopeTransaction (dbConnectionGetter:DbConnection.DbConnectionGetter) uid eNumber (tNumber:uint32) (amount:int) (date:System.DateTime) note =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "upd_EnvelopeTransaction"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid OdbcType.Int |> ignore
        AddParamWithTypeToCommand command "eNumber" eNumber OdbcType.SmallInt |> ignore
        AddParamWithTypeToCommand command "tNumber" tNumber OdbcType.Int |> ignore
        AddParamWithTypeToCommand command "amount" amount OdbcType.Int |> ignore
        AddParamWithTypeToCommand command "tDate" date OdbcType.DateTime |> ignore
        AddParamToCommand command "tNote" note |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }

let Del_EnvelopeTransaction (dbConnectionGetter:DbConnection.DbConnectionGetter) uid eNumber (tNumber:uint32) =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "del_EnvelopeTransaction"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid OdbcType.Int |> ignore
        AddParamWithTypeToCommand command "eNumber" eNumber OdbcType.SmallInt |> ignore
        AddParamWithTypeToCommand command "tNumber" tNumber OdbcType.Int |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }

let Sel_Envelope_Summary (dbConnectionGetter:DbConnection.DbConnectionGetter) uid =
    let getItemFromReader (reader:OdbcDataReader) =
        async {
            let! hasRow = reader.ReadAsync() |> Async.AwaitTask
            if hasRow then
                let a = (reader.GetInt32(2) |> System.Convert.ToDouble) / 100.0

                let e = { Number = reader.GetInt16(0) |> uint16; Name= reader.GetString(1); Amount = a }
                return Some e
            else
                return None
        }

    let rec recursivlyGetItems accList (reader:OdbcDataReader) =
        async {
            let! result = getItemFromReader reader
            match result with
            | Some r ->
                let newList = r :: accList
                return! recursivlyGetItems newList reader
            | None -> return accList
        }
    
    async {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "sel_Envelope_Summary"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid OdbcType.Int |> ignore

        do! connection.OpenAsync() |> Async.AwaitTask

        use! reader' = command.ExecuteReaderAsync() |> Async.AwaitTask
        let reader = reader' :?> OdbcDataReader

        let! result = recursivlyGetItems [] reader

        do! reader.CloseAsync() |> Async.AwaitTask
        do! connection.CloseAsync() |> Async.AwaitTask

        return result |> List.rev
    }

let Sel_Transactions (dbConnectionGetter:DbConnection.DbConnectionGetter) uid envelopeNumber limitNum page =
    let getItemFromReader (reader:OdbcDataReader) =
        async {
            let! hasRow = reader.ReadAsync() |> Async.AwaitTask
            if hasRow then
                let a = (reader.GetInt32(1) |> System.Convert.ToDouble) / 100.0

                let e = { TransactionNumber = reader.GetInt32(0) |> uint32; Amount = a; Date = reader.GetDateTime(2); Note = reader.GetString(3); }
                return Some e
            else
                return None
        }

    let rec recursivlyGetItems accList (reader:OdbcDataReader) =
        async {
            let! result = getItemFromReader reader
            match result with
            | Some r ->
                let newList = r :: accList
                return! recursivlyGetItems newList reader
            | None -> return accList
        }
    
    async {
    
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "sel_Transactions"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid OdbcType.Int |> ignore
        AddParamWithTypeToCommand command "eNumber" envelopeNumber OdbcType.SmallInt |> ignore
        AddParamWithTypeToCommand command "limitNum" limitNum OdbcType.Int |> ignore
        AddParamWithTypeToCommand command "offsetNum" (limitNum * (page-1u)) OdbcType.Int |> ignore
    
        do! connection.OpenAsync() |> Async.AwaitTask

        use! reader' = command.ExecuteReaderAsync() |> Async.AwaitTask
        let reader = reader' :?> OdbcDataReader

        let! transactions = recursivlyGetItems [] reader

        let! throwAway = reader.NextResultAsync() |> Async.AwaitTask
        let! throwAway = reader.ReadAsync() |> Async.AwaitTask
        let numOfTransactions = reader.GetInt32(0) |> uint32

        let result = { NumberOfAllTransactions = numOfTransactions; Transactions = transactions |> List.rev}

        do! reader.CloseAsync() |> Async.AwaitTask
        do! connection.CloseAsync() |> Async.AwaitTask

        return result
    }

let Sel_Transaction (dbConnectionGetter:DbConnection.DbConnectionGetter) uid envelopeNumber tNumber =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "sel_Transaction"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid OdbcType.Int |> ignore
        AddParamWithTypeToCommand command "eNumber" envelopeNumber OdbcType.SmallInt |> ignore
        AddParamWithTypeToCommand command "tNumber" tNumber OdbcType.Int |> ignore

        do! connection.OpenAsync()

        use! reader' = command.ExecuteReaderAsync()
        let reader = reader' :?> OdbcDataReader

        let! hasResult = reader.ReadAsync()

        let result = 
            if hasResult then
                let a = (reader.GetInt32(1) |> System.Convert.ToDouble) / 100.0

                Some { TransactionNumber = reader.GetInt32(0) |> uint32; Amount = a; Date = reader.GetDateTime(2); Note = reader.GetString(3); }
            else 
                None

        do! reader.CloseAsync()
        do! connection.CloseAsync()

        return result
    }

let Transfer (dbConnectionGetter:DbConnection.DbConnectionGetter) uid eSourceNumber eDestinationNumber amount =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "transfer"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid OdbcType.Int |> ignore
        AddParamWithTypeToCommand command "eSourceNumber" eSourceNumber OdbcType.SmallInt |> ignore
        AddParamWithTypeToCommand command "eDestinationNumber" eDestinationNumber OdbcType.SmallInt |> ignore
        AddParamWithTypeToCommand command "amount" amount OdbcType.Int |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }