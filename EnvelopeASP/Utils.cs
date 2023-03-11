using Microsoft.Extensions.Primitives;
using System.Security.Claims;

namespace EnvelopeASP
{
    public static class Utils
    {
        public const string LAYOUT = "_Layout";
        public const int DEFAULT_PAGINATION_SIZE = 50;
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

        public static bool RequestIsHTMX(HttpContext httpContext)
        {
            bool hasValue = httpContext.Request.Headers.TryGetValue("hx-request", out StringValues val);
            return hasValue && val.Equals("true");
        }

        public static string? HTMXRequestLayout(HttpContext httpContext) => RequestIsHTMX(httpContext) ? null : LAYOUT;
    }
}
