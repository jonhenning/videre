using System;
using System.Collections.Generic;
using System.Linq;
using CodeEndeavors.Extensions;
using Videre.Core.Models;

namespace Videre.Core.Services
{
    public class Security
    {
        public static List<Models.SecureActivity> GetSecureActivities(string area = null, string portalId = null)
        {
            return Repository.Current.GetResources<Models.SecureActivity>("SecureActivity").Select(m => m.Data).Where(a => 
                (string.IsNullOrEmpty(portalId) || a.PortalId == portalId) && 
                (string.IsNullOrEmpty(area) || a.Area.Equals(area, StringComparison.InvariantCultureIgnoreCase)) 
                ).OrderBy(a => a.Area).ThenBy(a => a.Name).ToList();
        }

        public static Models.SecureActivity GetSecureActivity(string portalId, string area, string name)
        {
            //todo: hack to allow portalid to be empty should be taken out!
            return GetSecureActivities().Where(r => (string.IsNullOrEmpty(r.PortalId) || r.PortalId.Equals(portalId, StringComparison.InvariantCultureIgnoreCase)) && area.Equals(r.Area, StringComparison.InvariantCultureIgnoreCase) && name.Equals(r.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
        public static Models.SecureActivity GetSecureActivityById(string Id)
        {
            return GetSecureActivities().Where(r => r.Id == Id).FirstOrDefault();
        }
        public static string Save(Models.SecureActivity activity, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            Validate(activity);
            var res = Repository.Current.StoreResource("SecureActivity", null, activity, userId);
            return res.Id;
        }
        public static void Validate(Models.SecureActivity activity)
        {
            if (string.IsNullOrEmpty(activity.PortalId) || string.IsNullOrEmpty(activity.Area) || string.IsNullOrEmpty(activity.Name))
                throw new Exception(Localization.GetExceptionText("InvalidResource.Error", "{0} is invalid.", "SecureActivity"));
            if (IsDuplicate(activity))
                throw new Exception(Localization.GetExceptionText("DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "SecureActivity"));
        }
        public static bool IsDuplicate(Models.SecureActivity activity)
        {
            var e = GetSecureActivity(activity.PortalId, activity.Area, activity.Name);
            return (e != null && e.Id != activity.Id);
        }
        public static bool Exists(Models.SecureActivity activity)
        {
            var m = GetSecureActivity(activity.PortalId, activity.Area, activity.Name);
            return (m != null);
        }
        public static bool DeleteSecureActivity(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var res = Repository.Current.GetResourceById<Models.SecureActivity>(id);
            if (res != null)
                Repository.Current.Delete(res);
            return res != null;
        }


        //todo: these functions are almost same, refactoring in order
        //additionally, the User object now takes care of IsActivityAuthorized... perhaps more refactoring in order here
        public static void VerifyActivityAuthorized(string area, string name, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            var activity = GetSecureActivity(portalId, area, name);
            if (activity != null)
            {
                if (!Authorization.IsAuthorized(Authentication.AuthenticatedUser, activity))
                    throw new Exception(Localization.GetLocalization(LocalizationType.Exception, "AccessDenied.Error", "Access Denied.", "Core"));
            }
        }
        public static bool IsActivityAuthorized(string area, string name, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            var activity = GetSecureActivity(portalId, area, name);
            if (activity != null)
                return Authorization.IsAuthorized(Authentication.AuthenticatedUser, activity); //Services.Account.IsInRole(activity.RoleIds, false);
            return false;
        }

        public static List<Models.SecureActivity> GetAuthorizedSecureActivities(string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;

            var user = Authorization.GetAuthorizationUser(userId);
            var activities = GetSecureActivities(portalId: Portal.CurrentPortalId);

            return activities.Where(a => Authorization.IsAuthorized(user, a)).ToList();
        }

        //public static List<Models.SecureActivity> GetAuthorizedSecureActivities()
        //{
        //    var activities = GetSecureActivities();
        //    return activities.Where(a => Services.Account.IsInRole(a.Roles, false)).ToList();
        //}

        //public static Dictionary<string, List<string>> GetActivityRoleNameDictionary(string area, string portalId = null)
        //{
        //    portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
        //    return GetSecureActivities().Where(a => a.PortalId == portalId && a.Area.Equals(area, StringComparison.InvariantCultureIgnoreCase)).ToDictionary(a => a.Name, a => a.Roles.Select(r => Account.GetRoleById(r).Name).ToList());
        //}

        public static List<string> GetNewRoleIds(List<string> roleIds, Dictionary<string, string> map)
        {
            var newRoleIds = new List<string>();
            if (roleIds != null)
            {
                foreach (var roleId in roleIds)
                {
                    var newRoleId = ImportExport.GetIdMap<Models.Role>(roleId, map);
                    if (!string.IsNullOrEmpty(newRoleId))
                        newRoleIds.Add(newRoleId);
                }
            }
            return newRoleIds;
        }

    }
}