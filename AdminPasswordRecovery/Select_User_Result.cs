namespace AdminPasswordRecovery
{
    internal readonly struct Select_User_Result
    {
        public readonly int UserID;
        public readonly int PasswordLength;
        public readonly int SaltLength;
        public readonly short MiB;
        public readonly short Iterations;
        public readonly short DoP;

        public Select_User_Result(int userID, int pl, int sl, short m, short i, short d)
        {
            UserID = userID;
            PasswordLength = pl;
            SaltLength = sl;
            MiB = m;
            Iterations = i;
            DoP = d;
        }
    }
}
