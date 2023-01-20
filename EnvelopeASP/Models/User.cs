namespace EnvelopeASP.Models
{
    public class User
    {
        public uint Id { get; set; }
        public string PasswordHash { get; set; }
        public DateTime LockoutExpiry { get; set; }

        public User(uint Id, string PasswordHash, DateTime LockoutExpiry) {
            this.Id = Id;
            this.PasswordHash = PasswordHash;
            this.LockoutExpiry = LockoutExpiry;
        }
    }
}
