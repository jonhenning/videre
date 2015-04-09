using CodeEndeavors.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Videre.Core.Models
{
    //a simple class to wrap what is stored in the principal
    public class AuthenticatedUser
    {
        public AuthenticatedUser()
        {
            RoleIds = new List<string>();
            Claims = new List<UserClaim>();
        }

        public string Id { get; set; }
        public List<string> RoleIds { get; set; }
        public List<Models.UserClaim> Claims { get; set; }

        public bool IsInRole(string roleId)
        {
            return RoleIds.Contains(roleId);
        }

        public bool HasClaim(UserClaim claim)
        {
            return HasClaim(claim.Issuer, claim.Type, claim.Value);
        }
        public bool HasClaim(string issuer, string type, string value)
        {
            var claims = GetClaims(type, issuer);
            return claims.Exists(c => c.Value.Equals(value, System.StringComparison.CurrentCultureIgnoreCase) || value == "*");
        }

        public UserClaim GetClaim(string type, string issuer)
        {
            return GetClaims(type, issuer).FirstOrDefault();
        }

        public List<UserClaim> GetClaims(string type, string issuer)
        {
            return Claims.Where(c => c.Type.Equals(type, System.StringComparison.InvariantCultureIgnoreCase) && c.Issuer.Equals(issuer, System.StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public T GetClaimValue<T>(string type, string issuer, T defaultValue)
        {
            var claim = GetClaim(type, issuer);
            if (claim != null)
                return claim.Value.ToType<T>();
            return defaultValue;
        }
    }
}
