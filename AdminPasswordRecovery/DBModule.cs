using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
namespace AdminPasswordRecovery
{
    internal static class DBModule
    {
        public static Select_User_Result? GetUserPasswordConfig(string connectionString, string email)
        {
            using var dbConnection = new MySqlConnection(connectionString);

            dbConnection.Open();

            var command = dbConnection.CreateCommand();
            command.CommandText = "SELECT UserID, LENGTH(PasswordHash) AS PasswordLength, LENGTH(PasswordSalt) AS SaltLength, Mib, Iterations, DegreeOfParallelism FROM `user` WHERE Email = ?e";
            command.Parameters.AddWithValue("e", email);

            var reader = command.ExecuteReader();

            bool hasRow = reader.Read();
            var userPasswordConfig = hasRow switch
            {
                true => new Select_User_Result(reader.GetUInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetByte(3), reader.GetByte(4), reader.GetByte(5)),
                false => new Select_User_Result?(),
            };
            reader.Close();
            dbConnection.Close();

            return userPasswordConfig;
        }

        public static int SetPassword(string connectionString, uint uid, byte[] newPassword, byte[] newSalt) 
        {
            using var dbConnection = new MySqlConnection(connectionString);

            dbConnection.Open();

            var command = dbConnection.CreateCommand();
            command.CommandText = "UPDATE `user` SET PasswordHash = ?p, PasswordSalt = ?s WHERE UserID = ?i";
            command.Parameters.AddWithValue("p", newPassword);
            command.Parameters.AddWithValue("s", newSalt);
            command.Parameters.AddWithValue("i", uid);

            int rowsEffected = command.ExecuteNonQuery();

            dbConnection.Close();

            return rowsEffected;
        }
    }
}
