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
        List<string> ExcludeRoleIds { get; set; }
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
        public static IAuthorizationUser GetAuthorizationUser(string userId)
        {
            return userId == Authentication.AuthenticatedUserId ? (IAuthorizationUser)Authentication.AuthenticatedUser : (IAuthorizationUser)Account.GetUserById(userId);
        }

        public static bool IsAuthorized(IAuthorizationEntity entity)
        {
            return IsAuthorized(Authentication.AuthenticatedUser, entity);
        }

        public static bool IsAuthorized(string userId, IAuthorizationEntity entity)
        {
            return IsAuthorized(GetAuthorizationUser(userId), entity);
        }

        public static bool IsAuthorized(IAuthorizationUser user, IAuthorizationEntity entity)
        {
            if (user != null)
                return IsAuthorized(user != null, user.RoleIds, user.Claims, entity.Authenticated, entity.RoleIds, entity.ExcludeRoleIds, entity.Claims);
            return IsAuthorized(false, null, null, entity.Authenticated, entity.RoleIds, entity.ExcludeRoleIds, entity.Claims);
        }

        public static bool IsAuthorized(IAuthorizationUser user, List<UserClaim> claims)
        {
            return claims.Exists(ec => user.Claims.Exists(uc => claimMatches(uc, ec)));
        }

        private static bool IsAuthorized(bool userAuthenticated, List<string> userRoleIds, List<UserClaim> userClaims, bool? entityAuthenticated, List<string> entityRoleIds, List<string> entityExcludeRoleIds, List<UserClaim> entityClaims)
        {
            if (userRoleIds == null)
                userRoleIds = new List<string>();
            if (userClaims == null)
                userClaims = new List<UserClaim>();
            if (entityRoleIds == null)
                entityRoleIds = new List<string>();
            if (entityExcludeRoleIds == null)
                entityExcludeRoleIds = new List<string>();
            if (entityClaims == null)
                entityClaims = new List<UserClaim>();

            if (entityAuthenticated == false && userAuthenticated)
                return false;
            if (entityAuthenticated == true && userAuthenticated == false)
                return false;
            
            if (entityExcludeRoleIds.Exists(id => userRoleIds.Contains(id)))    //if exlcude role exists then not authorized regardless of other roles/claims
                return false;

            if (entityRoleIds.Count == 0 && entityClaims.Count == 0)    //if no permissions assigned to resource your in!
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
