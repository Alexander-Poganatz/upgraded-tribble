namespace EnvelopeASP.Models
{
    public class Sel_Transactions_Result
    {
        public uint NumberOfAllTransactions { get; private set; }
        public List<Transaction> Transactions { get; private set; }

        public Sel_Transactions_Result(uint numberOfAllTransactions, List<Transaction> transactions)
        {
            NumberOfAllTransactions = numberOfAllTransactions;
            Transactions = transactions;
        }
    }
}
