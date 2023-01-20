namespace EnvelopeASP.Models
{
    public class Transaction
    {
        public uint TransactionNumber { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }

        public Transaction()
        {
            Note = string.Empty;
        }

        public Transaction(uint transactionNumber, double amount, DateTime date, string note)
        {
            TransactionNumber = transactionNumber;
            Amount = amount;
            Date = date;
            Note = note;
        }
    }
}
