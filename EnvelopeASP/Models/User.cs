namespace EnvelopeASP.Models
{
    public sealed class User
    {
        public uint Id { get; private set; }
        public byte[] PasswordHash { get; private set; }
        public DateTime LockoutExpiry { get; private set; }
        public PasswordConfig PasswordConfig { get; private set; }

        public User(uint Id, byte[] PasswordHash, DateTime LockoutExpiry, PasswordConfig passwordConfig)
        {
            this.Id = Id;
            this.PasswordHash = PasswordHash;
            this.LockoutExpiry = LockoutExpiry;
            PasswordConfig = passwordConfig;
        }
    }
}
