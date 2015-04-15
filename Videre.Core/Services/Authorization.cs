using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Videre.Core.Models;

namespace Videre.Core.Services
{
    public interface IAuthorizationEntity
    {
        bool? Authenticated { get; set; }
        List<string> RoleIds { get; set; }
        List<UserClaim> Claims { get; set; }
    }

    public interface IAuthorizationUser
    {
        string Id { get; set; }
        List<string> RoleIds { get; set; }
        List<UserClaim> Claims { get; set; }
    }

    public class Authorization
    {
        public static bool IsAuthorized(IAuthorizationUser user, IAuthorizationEntity entity)
        {
            if (user != null)
                return IsAuthorized(user != null, user.RoleIds, user.Claims, entity.Authenticated, entity.RoleIds, entity.Claims);
            return IsAuthorized(false, null, null, entity.Authenticated, entity.RoleIds, entity.Claims);
        }

        private static bool IsAuthorized(bool userAuthenticated, List<string> userRoleIds, List<UserClaim> userClaims, bool? entityAuthenticated, List<string> entityRoleIds, List<UserClaim> entityClaims)
        {
            if (userRoleIds == null)
                userRoleIds = new List<string>();
            if (userClaims == null)
                userClaims = new List<UserClaim>();
            if (entityRoleIds == null)
                entityRoleIds = new List<string>();
            if (entityClaims == null)
                entityClaims = new List<UserClaim>();

            if (entityAuthenticated == false && userAuthenticated)
                return false;
            if (entityAuthenticated == true && !userAuthenticated)
                return false;

            if (userRoleIds.Count == 0 && userClaims.Count == 0)
                return true;

            if (userAuthenticated)
            {
                //if both roles and claims have values, they only need a match against either
                if (entityRoleIds.Count > 0 && entityClaims.Count > 0)
                    return entityRoleIds.Exists(id => userRoleIds.Contains(id)) || entityClaims.Exists(ec => userClaims.Exists(uc => claimMatches(uc, ec)));
                if (entityRoleIds.Count > 0)
                    return entityRoleIds.Exists(id => userRoleIds.Contains(id));
                if (entityClaims.Count > 0)
                    return entityClaims.Exists(ec => userClaims.Exists(uc => claimMatches(uc, ec)));
            }

            return false;
        }

        private static bool claimMatches(UserClaim claim, UserClaim compareToClaim)
        {
            return claim.Issuer == compareToClaim.Issuer && claim.Type == compareToClaim.Type && (claim.Value == compareToClaim.Value || compareToClaim.Value == "*");
        }


    }
}
