module Procedures

open MySqlConnector
open Models

let private AddParamToCommand (command:MySqlCommand) (paramName:string) (value:obj) =
    let param = MySqlParameter(paramName, value)
    command.Parameters.Add param

let private AddParamWithTypeToCommand (command:MySqlCommand) (paramName:string) (value:obj) mySqlType =
    let param = MySqlParameter(paramName, value)
    param.MySqlDbType <- mySqlType
    command.Parameters.Add param

let InsertUser (dbConnectionGetter:DbConnection.IDbConnectionGetter) email passwordHash (passwordConfig:PasswordConfig) =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "ins_User"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamToCommand command "e" email |> ignore
        AddParamWithTypeToCommand command "p" passwordHash MySqlDbType.Binary |> ignore
        AddParamWithTypeToCommand command "s" passwordConfig.Salt MySqlDbType.Binary |> ignore
        AddParamWithTypeToCommand command "m" passwordConfig.MiB MySqlDbType.UByte |> ignore
        AddParamWithTypeToCommand command "i" passwordConfig.Iterations MySqlDbType.UByte |> ignore
        AddParamWithTypeToCommand command "dop" passwordConfig.DegreeOfParallism MySqlDbType.UByte |> ignore

    
        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }
    
let UpdateUserPassword (dbConnectionGetter:DbConnection.IDbConnectionGetter) uid passwordHash (passwordConfig:PasswordConfig) =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "upd_User_Password"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uid" uid MySqlDbType.UInt32 |> ignore
        AddParamWithTypeToCommand command "p" passwordHash MySqlDbType.Binary |> ignore
        AddParamWithTypeToCommand command "s" passwordConfig.Salt MySqlDbType.Binary |> ignore
        AddParamWithTypeToCommand command "m" passwordConfig.MiB MySqlDbType.UByte |> ignore
        AddParamWithTypeToCommand command "i" passwordConfig.Iterations MySqlDbType.UByte |> ignore
        AddParamWithTypeToCommand command "dop" passwordConfig.DegreeOfParallism MySqlDbType.UByte |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }

let Sel_UserByEmail (dbConnectionGetter:DbConnection.IDbConnectionGetter) email =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "sel_UserByEmail"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamToCommand command "e" email |> ignore

    
        do! connection.OpenAsync()

        use! reader' = command.ExecuteReaderAsync()
        let reader = reader' :?> MySqlDataReader

        let! userExists = reader.ReadAsync()

        let user = 
            if userExists then
                let passwordHash = Array.create Password.HashOutputSize 0uy
                let salt = Array.create Password.SaltSize 0uy
                reader.GetBytes(1, 0, passwordHash, 0, passwordHash.Length) |> ignore
                reader.GetBytes(2, 0, salt, 0, salt.Length) |> ignore
                let passwordConfig = { MiB = reader.GetByte(3); Iterations = reader.GetByte(4); DegreeOfParallism = reader.GetByte(5); Salt = salt; }
                let user = { Id = reader.GetUInt32(0); PasswordHash = passwordHash; LockoutExpiry = reader.GetDateTime(6); PasswordConfig = passwordConfig }
                Some user
            else
                None

        do! reader.CloseAsync()
        do! connection.CloseAsync()

        return user
    }

let Upd_User_Login (dbConnectionGetter:DbConnection.IDbConnectionGetter) uid isValid =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "upd_User_Login"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid MySqlDbType.UInt32 |> ignore
        AddParamWithTypeToCommand command "isValid" isValid MySqlDbType.Bit |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }

let Ins_Envelope (dbConnectionGetter:DbConnection.IDbConnectionGetter) uid eName =
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

let Upd_Envelope (dbConnectionGetter:DbConnection.IDbConnectionGetter) uid eNumber newName =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "upd_Envelope"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid MySqlDbType.UInt32 |> ignore
        AddParamWithTypeToCommand command "eNumber" eNumber MySqlDbType.UInt16 |> ignore
        AddParamToCommand command "eName" newName |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }

let Del_Envelope (dbConnectionGetter:DbConnection.IDbConnectionGetter) uid eNumber =
    task {
    
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "del_Envelope"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid MySqlDbType.UInt32 |> ignore
        AddParamWithTypeToCommand command "eNumber" eNumber MySqlDbType.UInt16 |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }

let Ins_EnvelopeTransaction (dbConnectionGetter:DbConnection.IDbConnectionGetter) uid eNumber (amount:int) (date:System.DateTime) note =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "ins_EnvelopeTransaction"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid MySqlDbType.UInt32 |> ignore
        AddParamWithTypeToCommand command "eNumber" eNumber MySqlDbType.UInt16 |> ignore
        AddParamWithTypeToCommand command "amount" amount MySqlDbType.Int32 |> ignore
        AddParamWithTypeToCommand command "tDate" date MySqlDbType.DateTime |> ignore
        AddParamToCommand command "tNote" note |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync() |> Async.AwaitTask

        do! connection.CloseAsync() |> Async.AwaitTask

        return rowsInserted
    }

let Upd_EnvelopeTransaction (dbConnectionGetter:DbConnection.IDbConnectionGetter) uid eNumber (tNumber:uint32) (amount:int) (date:System.DateTime) note =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "upd_EnvelopeTransaction"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid MySqlDbType.UInt32 |> ignore
        AddParamWithTypeToCommand command "eNumber" eNumber MySqlDbType.UInt16 |> ignore
        AddParamWithTypeToCommand command "tNumber" tNumber MySqlDbType.UInt32 |> ignore
        AddParamWithTypeToCommand command "amount" amount MySqlDbType.Int32 |> ignore
        AddParamWithTypeToCommand command "tDate" date MySqlDbType.DateTime |> ignore
        AddParamToCommand command "tNote" note |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }

let Del_EnvelopeTransaction (dbConnectionGetter:DbConnection.IDbConnectionGetter) uid eNumber (tNumber:uint32) =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "del_EnvelopeTransaction"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid MySqlDbType.UInt32 |> ignore
        AddParamWithTypeToCommand command "eNumber" eNumber MySqlDbType.UInt16 |> ignore
        AddParamWithTypeToCommand command "tNumber" tNumber MySqlDbType.UInt32 |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }

let Sel_Envelope_Summary (dbConnectionGetter:DbConnection.IDbConnectionGetter) uid =
    let getItemFromReader (reader:MySqlDataReader) =
        async {
            let! hasRow = reader.ReadAsync() |> Async.AwaitTask
            if hasRow then
                let a = (reader.GetInt32(2) |> System.Convert.ToDouble) / 100.0

                let e = { Number = reader.GetUInt16(0); Name= reader.GetString(1); Amount = a }
                return Some e
            else
                return None
        }

    let rec recursivlyGetItems accList (reader:MySqlDataReader) =
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

        AddParamWithTypeToCommand command "uID" uid MySqlDbType.UInt32 |> ignore

        do! connection.OpenAsync() |> Async.AwaitTask

        use! reader' = command.ExecuteReaderAsync() |> Async.AwaitTask
        let reader = reader' :?> MySqlDataReader

        let! result = recursivlyGetItems [] reader

        do! reader.CloseAsync() |> Async.AwaitTask
        do! connection.CloseAsync() |> Async.AwaitTask

        return result |> List.rev
    }

let Sel_Transactions (dbConnectionGetter:DbConnection.IDbConnectionGetter) uid envelopeNumber limitNum page =
    let getItemFromReader (reader:MySqlDataReader) =
        async {
            let! hasRow = reader.ReadAsync() |> Async.AwaitTask
            if hasRow then
                let a = (reader.GetInt32(1) |> System.Convert.ToDouble) / 100.0

                let e = { TransactionNumber = reader.GetUInt32(0); Amount = a; Date = reader.GetDateTime(2); Note = reader.GetString(3); }
                return Some e
            else
                return None
        }

    let rec recursivlyGetItems accList (reader:MySqlDataReader) =
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

        AddParamWithTypeToCommand command "uID" uid MySqlDbType.UInt32 |> ignore
        AddParamWithTypeToCommand command "eNumber" envelopeNumber MySqlDbType.UInt16 |> ignore
        AddParamWithTypeToCommand command "limitNum" limitNum MySqlDbType.UInt32 |> ignore
        AddParamWithTypeToCommand command "offsetNum" (limitNum * (page-1u)) MySqlDbType.UInt32 |> ignore
    
        do! connection.OpenAsync() |> Async.AwaitTask

        use! reader' = command.ExecuteReaderAsync() |> Async.AwaitTask
        let reader = reader' :?> MySqlDataReader

        let! transactions = recursivlyGetItems [] reader

        let! throwAway = reader.NextResultAsync() |> Async.AwaitTask
        let! throwAway = reader.ReadAsync() |> Async.AwaitTask
        let numOfTransactions = reader.GetUInt32(0)

        let result = { NumberOfAllTransactions = numOfTransactions; Transactions = transactions |> List.rev}

        do! reader.CloseAsync() |> Async.AwaitTask
        do! connection.CloseAsync() |> Async.AwaitTask

        return result
    }

let Sel_Transaction (dbConnectionGetter:DbConnection.IDbConnectionGetter) uid envelopeNumber tNumber =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "sel_Transaction"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid MySqlDbType.UInt32 |> ignore
        AddParamWithTypeToCommand command "eNumber" envelopeNumber MySqlDbType.UInt16 |> ignore
        AddParamWithTypeToCommand command "tNumber" tNumber MySqlDbType.UInt32 |> ignore

        do! connection.OpenAsync()

        use! reader' = command.ExecuteReaderAsync()
        let reader = reader' :?> MySqlDataReader

        let! hasResult = reader.ReadAsync()

        let result = 
            if hasResult then
                let a = (reader.GetInt32(1) |> System.Convert.ToDouble) / 100.0

                Some { TransactionNumber = reader.GetUInt32(0); Amount = a; Date = reader.GetDateTime(2); Note = reader.GetString(3); }
            else 
                None

        do! reader.CloseAsync()
        do! connection.CloseAsync()

        return result
    }

let Transfer (dbConnectionGetter:DbConnection.IDbConnectionGetter) uid eSourceNumber eDestinationNumber amount =
    task {
        use connection = dbConnectionGetter.GetNewConnection()

        let command = connection.CreateCommand()

        command.CommandText <- "transfer"
        command.CommandType <- System.Data.CommandType.StoredProcedure

        AddParamWithTypeToCommand command "uID" uid MySqlDbType.UInt32 |> ignore
        AddParamWithTypeToCommand command "eSourceNumber" eSourceNumber MySqlDbType.UInt16 |> ignore
        AddParamWithTypeToCommand command "eDestinationNumber" eDestinationNumber MySqlDbType.UInt16 |> ignore
        AddParamWithTypeToCommand command "amount" amount MySqlDbType.Int32 |> ignore

        do! connection.OpenAsync()

        let! rowsInserted = command.ExecuteNonQueryAsync()

        do! connection.CloseAsync()

        return rowsInserted
    }