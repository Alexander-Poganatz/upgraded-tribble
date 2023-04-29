using MySqlConnector;
using System.Data;

namespace EnvelopeASP.Models
{
    public static class Procedures
    {
        private static string _connectionString = string.Empty;
        public static void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static async Task<bool> InsertUser(string email, byte[] passwordHash, PasswordConfig passwordConfig)
        {
            using var connection = new MySqlConnection(_connectionString);

            using var command = connection.CreateCommand();

            command.CommandText = "ins_User";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("e", email));
            command.Parameters.Add(new MySqlParameter("p", passwordHash) { MySqlDbType = MySqlDbType.Binary });
            command.Parameters.Add(new MySqlParameter("s", passwordConfig.Salt) { MySqlDbType = MySqlDbType.Binary });
            command.Parameters.Add(new MySqlParameter("m", passwordConfig.MiB));
            command.Parameters.Add(new MySqlParameter("i", passwordConfig.Iterations));
            command.Parameters.Add(new MySqlParameter("dop", passwordConfig.DegreeOfParallelism));

            var openTask = connection.OpenAsync();

            await openTask;

            var rowsInserted = await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();

            return rowsInserted > 0;
        }

        public static async Task<bool> UpdateUserPassword(uint uid, byte[] passwordHash, PasswordConfig passwordConfig)
        {
            using var connection = new MySqlConnection(_connectionString);

            using var command = connection.CreateCommand();

            command.CommandText = "upd_User_Password";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("uid", uid));
            command.Parameters.Add(new MySqlParameter("p", passwordHash) { MySqlDbType = MySqlDbType.Binary });
            command.Parameters.Add(new MySqlParameter("s", passwordConfig.Salt) { MySqlDbType = MySqlDbType.Binary });
            command.Parameters.Add(new MySqlParameter("m", passwordConfig.MiB));
            command.Parameters.Add(new MySqlParameter("i", passwordConfig.Iterations));
            command.Parameters.Add(new MySqlParameter("dop", passwordConfig.DegreeOfParallelism));

            var openTask = connection.OpenAsync();

            await openTask;

            var rowsInserted = await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();

            return rowsInserted > 0;
        }

        public static async Task<User?> Sel_UserByEmail(string email)
        {
            using var connection = new MySqlConnection(_connectionString);
            var openTask = connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = "sel_UserByEmail";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("e", email));

            await openTask;

            using var reader = await command.ExecuteReaderAsync();

            User? user = null;

            while(reader.Read()) {
                byte[] passwordHash = new byte[PasswordConfig.HASH_OUTPUT_SIZE];
                byte[] salt = new byte[PasswordConfig.SALT_SIZE];
                reader.GetBytes(1, 0, passwordHash, 0, passwordHash.Length);
                reader.GetBytes(2, 0, salt, 0, salt.Length);
                var passwordConfig = new PasswordConfig(reader.GetByte(3), reader.GetByte(4), reader.GetByte(5), salt);
                user = new User(reader.GetUInt32(0), passwordHash, reader.GetDateTime(6), passwordConfig);
            }

            await reader.CloseAsync();
            await connection.CloseAsync();

            return user;

        }

        public static async Task Upd_User_Login(uint uID, bool isValid)
        {
            using var connection = new MySqlConnection(_connectionString);
            var openTask = connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = "upd_User_Login";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("uID", uID));
            command.Parameters.Add(new MySqlParameter("isValid", isValid));

            await openTask;

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();


        }

        public static async Task<ushort> Ins_Envelope(uint uID, string eName)
        {
            using var connection = new MySqlConnection(_connectionString);
            var openTask = connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = "ins_Envelope";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("uID", uID));
            command.Parameters.Add(new MySqlParameter("eName", eName));

            await openTask;

            using var reader = await command.ExecuteReaderAsync();

            ushort newENumber = 0;

            while(await reader.ReadAsync())
            {
                newENumber = Convert.ToUInt16(reader[0]);
            }
            await reader.CloseAsync();
            await connection.CloseAsync();

            return newENumber;
        }

        public static async Task Upd_Envelope(uint uID, ushort eNumber, string newName)
        {
            using var connection = new MySqlConnection(_connectionString);
            var openTask = connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = "upd_Envelope";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("uID", uID));
            command.Parameters.Add(new MySqlParameter("eNumber", eNumber));
            command.Parameters.Add(new MySqlParameter("eName", newName));

            await openTask;

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();

        }

        public static async Task Del_Envelope(uint uID, ushort eNumber)
        {
            using var connection = new MySqlConnection(_connectionString);
            var openTask = connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = "del_Envelope";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("uID", uID));
            command.Parameters.Add(new MySqlParameter("eNumber", eNumber));

            await openTask;

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();

        }

        public static async Task Ins_EnvelopeTransaction(uint uID, ushort eNumber, int amount, DateTime date, string note)
        {
            using var connection = new MySqlConnection(_connectionString);
            var openTask = connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = "ins_EnvelopeTransaction";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("uID", uID));
            command.Parameters.Add(new MySqlParameter("eNumber", eNumber));
            command.Parameters.Add(new MySqlParameter("amount", amount));
            command.Parameters.Add(new MySqlParameter("tDate", date));
            command.Parameters.Add(new MySqlParameter("tNote", note));

            await openTask;

            var tNumber = Convert.ToUInt32(await command.ExecuteScalarAsync());

            await connection.CloseAsync();

        }

        public static async Task Upd_EnvelopeTransaction(uint uID, ushort eNumber, uint tNumber, int amount, DateTime date, string note)
        {
            using var connection = new MySqlConnection(_connectionString);
            var openTask = connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = "upd_EnvelopeTransaction";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("uID", uID));
            command.Parameters.Add(new MySqlParameter("eNumber", eNumber));
            command.Parameters.Add(new MySqlParameter("tNumber", tNumber));
            command.Parameters.Add(new MySqlParameter("amount", amount));
            command.Parameters.Add(new MySqlParameter("tDate", date));
            command.Parameters.Add(new MySqlParameter("tNote", note));

            await openTask;

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();

        }

        public static async Task Del_EnvelopeTransaction(uint uID, ushort eNumber, uint tNumber)
        {
            using var connection = new MySqlConnection(_connectionString);
            var openTask = connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = "del_EnvelopeTransaction";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("uID", uID));
            command.Parameters.Add(new MySqlParameter("eNumber", eNumber));
            command.Parameters.Add(new MySqlParameter("tNumber", tNumber));

            await openTask;

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();

        }

        public static async Task<List<Envelope>> Sel_Envelope_Summary(uint uID)
        {
            using var connection = new MySqlConnection(_connectionString);
            var openTask = connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = "sel_Envelope_Summary";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("uID", uID));

            await openTask;

            using var reader = await command.ExecuteReaderAsync();

            var envelopes = new List<Envelope>();

            while (reader.Read())
            {
                double a = Convert.ToDouble(reader.GetInt32(2)) / 100.0;
                
                var e = new Envelope(reader.GetUInt16(0), reader.GetString(1), a);
                envelopes.Add(e);
            }

            await reader.CloseAsync();
            await connection.CloseAsync();

            return envelopes;
        }

        public static async Task<Sel_Transactions_Result> Sel_Transactions(uint uID, ushort envelopeNumber, uint limitNum, uint page)
        {
            using var connection = new MySqlConnection(_connectionString);
            var openTask = connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = "sel_Transactions";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("uID", uID));
            command.Parameters.Add(new MySqlParameter("eNumber", envelopeNumber));
            command.Parameters.Add(new MySqlParameter("limitNum", limitNum));
            command.Parameters.Add(new MySqlParameter("offsetNum", limitNum * (page-1)));

            await openTask;

            using var reader = await command.ExecuteReaderAsync();

            var transactions = new List<Transaction>();
            
            while (await reader.ReadAsync())
            {
                double a = Convert.ToDouble(reader.GetInt32(1)) / 100.0;

                var e = new Transaction(reader.GetUInt32(0), a, reader.GetDateTime(2), reader.GetString(3));
                transactions.Add(e);
            }
            await reader.NextResultAsync();
            await reader.ReadAsync();
            var numOfTransactions = Convert.ToUInt32(reader[0]);

            await reader.CloseAsync();
            await connection.CloseAsync();

            return new Sel_Transactions_Result(numOfTransactions, transactions);
        }

        public static async Task<Transaction?> Sel_Transaction(uint uID, ushort envelopeNumber, uint tNumber)
        {
            using var connection = new MySqlConnection(_connectionString);
            var openTask = connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = "sel_Transaction";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("uID", uID));
            command.Parameters.Add(new MySqlParameter("eNumber", envelopeNumber));
            command.Parameters.Add(new MySqlParameter("tNumber", tNumber));

            await openTask;

            using var reader = await command.ExecuteReaderAsync();

            Transaction? transaction = null;

            while (reader.Read())
            {
                double a = Convert.ToDouble(reader.GetInt32(1)) / 100.0;

                transaction = new Transaction(reader.GetUInt32(0), a, reader.GetDateTime(2), reader.GetString(3));
                
            }

            await reader.CloseAsync();
            await connection.CloseAsync();

            return transaction;
        }

        public static async Task<bool> Transfer(uint uID, ushort eSourceNumber, ushort eDestinationNumber, int amount)
        {
            using var connection = new MySqlConnection(_connectionString);
            var openTask = connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = "transfer";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("uID", uID));
            command.Parameters.Add(new MySqlParameter("eSourceNumber", eSourceNumber));
            command.Parameters.Add(new MySqlParameter("eDestinationNumber", eDestinationNumber));
            command.Parameters.Add(new MySqlParameter("amount", amount));

            await openTask;

            var tNumber = Convert.ToInt32(await command.ExecuteScalarAsync());

            await connection.CloseAsync();

            return tNumber > 0;
        }
    }
}
