using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminPasswordRecovery
{
    internal readonly struct Select_User_Result
    {
        public readonly uint UserID;
        public readonly int PasswordLength;
        public readonly int SaltLength;
        public readonly byte MiB;
        public readonly byte Iterations;
        public readonly byte DoP;

        public Select_User_Result(uint userID, int pl, int sl, byte m, byte i, byte d)
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
