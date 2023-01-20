using System.Security.Claims;

namespace EnvelopeASP
{
    public static class Utils
    {

        public static uint GetUserIDFromClaims(ClaimsPrincipal principal)
        {
            var idClaim = principal.Claims.First(f => f.Type == ClaimTypes.NameIdentifier);

            var val = Convert.ToUInt32(idClaim.Value);

            return val;
        }

        public static int DoubleMoneyToCents(double dbl)
        {
            var intPart = Convert.ToInt32(dbl);

            var decimalPartAsInt = Convert.ToInt32(Convert.ToDouble(dbl - intPart) * 100);
            return (intPart * 100) + decimalPartAsInt;
        }
    }
}
