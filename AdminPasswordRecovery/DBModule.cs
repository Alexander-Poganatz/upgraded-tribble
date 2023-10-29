using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Odbc;
namespace AdminPasswordRecovery
{
    internal static class DBModule
    {
        public static Select_User_Result? GetUserPasswordConfig(string connectionString, string email)
        {
            using var dbConnection = new OdbcConnection(connectionString);

            dbConnection.Open();

            var command = dbConnection.CreateCommand();
            command.CommandText = "SELECT UserID, LENGTH(PasswordHash) AS PasswordLength, LENGTH(PasswordSalt) AS SaltLength, Mib, Iterations, DegreeOfParallelism FROM `User` WHERE Email = ?";
            command.Parameters.AddWithValue("e", email);

            var reader = command.ExecuteReader();

            bool hasRow = reader.Read();
            var userPasswordConfig = hasRow switch
            {
                true => new Select_User_Result(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt16(3), reader.GetInt16(4), reader.GetInt16(5)),
                false => new Select_User_Result?(),
            };
            reader.Close();
            dbConnection.Close();

            return userPasswordConfig;
        }

        public static int SetPassword(string connectionString, int uid, byte[] newPassword, byte[] newSalt) 
        {
            using var dbConnection = new OdbcConnection(connectionString);

            dbConnection.Open();

            var command = dbConnection.CreateCommand();
            command.CommandText = "UPDATE `User` SET PasswordHash = ?, PasswordSalt = ?, LockoutExpiry = '2022-01-01', FailedPasswordCount = 0 WHERE UserID = ?";
            command.Parameters.AddWithValue("p", newPassword);
            command.Parameters.AddWithValue("s", newSalt);
            command.Parameters.AddWithValue("i", uid);

            int rowsEffected = command.ExecuteNonQuery();

            dbConnection.Close();

            return rowsEffected;
        }
    }
}
