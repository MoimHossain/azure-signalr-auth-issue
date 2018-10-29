using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace System.Security.Claims
{
    public static class ClaimsPrincipalExtensions
    {
        private const string _objectIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        public static string GetObjectId(this ClaimsPrincipal principal)
        {
            var claim = principal.Claims.FirstOrDefault(c => c.Type == _objectIdClaimType);
            return claim != null ? claim.Value : null;
        }
    }
}
