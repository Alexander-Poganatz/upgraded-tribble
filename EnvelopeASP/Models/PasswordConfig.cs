namespace EnvelopeASP.Models
{
    /// <summary>
    /// Not the sign up model, its for storing all the details related to an argon 2 password
    /// </summary>
    public sealed class PasswordConfig
    {
        public const int SALT_SIZE = 16;
        public const int HASH_OUTPUT_SIZE = 32;
        public byte MiB { get; private set; }
        public byte Iterations { get; private set; }
        public byte DegreeOfParallelism { get; private set; }
        public byte[] Salt { get; private set; }

        public PasswordConfig() {
            MiB = 19;
            Iterations = 2;
            DegreeOfParallelism = 1;
            Salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(SALT_SIZE);
        }

        public PasswordConfig(byte miB, byte iterations, byte degreeOfParallelism, byte[] salt)
        {
            MiB = miB;
            Iterations = iterations;
            DegreeOfParallelism = degreeOfParallelism;
            Salt = salt;
        }

        private static byte[] PasswordToBytes(string password) => System.Text.Encoding.UTF8.GetBytes(password.Trim());

        public byte[] GenerateHash(string password)
        {
            var argon = new Konscious.Security.Cryptography.Argon2id(PasswordToBytes(password));
            argon.MemorySize = 1024 * Convert.ToInt32(MiB);
            argon.Iterations = Iterations;
            argon.DegreeOfParallelism = DegreeOfParallelism;
            argon.Salt = Salt;
            return argon.GetBytes(HASH_OUTPUT_SIZE);
        }

        public bool Verify(string password, byte[] dbHash) => GenerateHash(password).SequenceEqual(dbHash);
        
    }
}
