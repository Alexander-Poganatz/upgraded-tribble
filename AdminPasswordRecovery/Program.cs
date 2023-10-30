
using AdminPasswordRecovery;

Console.WriteLine("Welcome to Envelope Password Override");

if(args.Length < 2)
{
    Console.WriteLine("Expected at least 2 arguments: DB_Connection_String email");
    return 1;
}

try
{

    string connectionString = args[0];

    string email = args[1];

    var existingPasswordConfigNul = DBModule.GetUserPasswordConfig(connectionString, email);
    if (existingPasswordConfigNul.HasValue == false)
    {
        Console.WriteLine("Existing user not found.");
        return 1;
    }
    var existingPasswordConfig = existingPasswordConfigNul.Value;
    byte[] newSalt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(existingPasswordConfig.SaltLength);
    var base64Password = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

    var argon = new Konscious.Security.Cryptography.Argon2id(System.Text.Encoding.UTF8.GetBytes(base64Password));
    argon.MemorySize = 1024 * Convert.ToInt32(existingPasswordConfig.MiB);
    argon.Iterations = existingPasswordConfig.Iterations;
    argon.DegreeOfParallelism = existingPasswordConfig.DoP;
    argon.Salt = newSalt;
    var newPasswordHash = argon.GetBytes(existingPasswordConfig.PasswordLength);

    int rowsEffected = DBModule.SetPassword(connectionString, newPasswordHash, newSalt, existingPasswordConfig);

    Console.WriteLine("Effected {0} rows!", rowsEffected);
    Console.WriteLine("New Password: {0}", base64Password);

} catch(Exception ex)
{
    Console.Error.WriteLine(ex.StackTrace);
    Console.Error.WriteLine(ex.Message);
    return 2;
}


return 0;
