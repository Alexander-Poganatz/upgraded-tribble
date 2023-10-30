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
            //command.CommandText = "SELECT UserID, LENGTH(PasswordHash) AS PasswordLength, LENGTH(PasswordSalt) AS SaltLength, Mib, Iterations, DegreeOfParallelism FROM `User` WHERE Email = ?";
            command.CommandText = "{CALL sel_UserByEmail(?)}";
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("e", email);

            var reader = command.ExecuteReader();

            bool hasRow = reader.Read();

            Select_User_Result? userPasswordConfig = null;
            if (hasRow)
            {
                var lengthOfPassword = Convert.ToInt16(reader.GetBytes(1, 0, null, 0, 0));
                var lengthOfSalt = Convert.ToInt16(reader.GetBytes(2, 0, null, 0, 0));
                userPasswordConfig = new Select_User_Result(reader.GetInt32(0), lengthOfPassword, lengthOfSalt, reader.GetInt16(3), reader.GetInt16(4), reader.GetInt16(5));
            }

            reader.Close();
            dbConnection.Close();

            return userPasswordConfig;
        }

        public static int SetPassword(string connectionString, byte[] newPassword, byte[] newSalt, Select_User_Result userData) 
        {
            using var dbConnection = new OdbcConnection(connectionString);

            dbConnection.Open();

            var command = dbConnection.CreateCommand();
            command.CommandText = "{CALL upd_User_Password(?,?,?,?,?,?)}";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("i", userData.UserID);
            command.Parameters.AddWithValue("p", newPassword);
            command.Parameters.AddWithValue("s", newSalt);
            command.Parameters.AddWithValue("m", userData.MiB);
            command.Parameters.AddWithValue("i", userData.Iterations);
            command.Parameters.AddWithValue("dop", userData.DoP);

            int rowsEffected = command.ExecuteNonQuery();

            dbConnection.Close();

            return rowsEffected;
        }
    }
}
