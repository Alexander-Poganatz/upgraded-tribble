namespace EnvelopeASP.Models
{
    public class Envelope
    {
        public ushort Number { get; set; }
        public string Name { get; set; }
        public double Amount { get; set; }

        public Envelope() { 
            Name= string.Empty;
        }

        public Envelope(ushort number, string name, double amount) {
            Number = number;
            Name = name;
            Amount = amount;
        }
    }
}
