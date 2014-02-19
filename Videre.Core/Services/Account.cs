using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using Videre.Core.Models;
using CodeEndeavors.Extensions;
using StructureMap;
using System.Configuration;
using System.Security.Cryptography;

namespace Videre.Core.Services
{
    public class Account
    {
        private static AccountProviders.IAccountService _accountService;

        private static AccountProviders.IAccountService AccountService
        {
            get
            {
                if (_accountService == null)
                {
                    _accountService = ConfigurationManager.AppSettings.GetSetting("AccountServicesProvider", "Videre.Core.AccountProviders.VidereAccount, Videre.Core").GetInstance<AccountProviders.IAccountService>();
                    _accountService.Initialize(ConfigurationManager.AppSettings.GetSetting("AccountServicesConnection", ""));
                }
                return _accountService;
            }
        }

        public static List<Models.CustomDataElement> CustomUserElements
        {
            get
            {
                return AccountService.CustomUserElements;
            }
        }

        [Obsolete("Use Authentication.IsAuthenticated")]
        public static bool IsAuthenticated { get { return Authentication.IsAuthenticated; } }
        [Obsolete("Use Authentication.AuthenticatedUserId")]
        public static string CurrentIdentityName { get { return Authentication.AuthenticatedUserId; } }
        public static string AuditId { get { return Authentication.AuthenticatedUserId; } }

        public static void VerifyInRole(string roleId)
        {
            IsInRole(new List<string>() { roleId }, true);
        }

        public static void VerifyInRole(List<string> roleIds)
        {
            IsInRole(roleIds, true);
        }

        public static bool IsInRole(string userId, string roleId)
        {
            return IsInRole(userId, new List<string>() { roleId });
        }

        public static bool IsInRole(string roleId, bool throwException = false)
        {
            return IsInRole(new List<string>() { roleId }, throwException);
        }

        public static bool IsInRoleNames(string userId, List<string> roleNames)
        {
            return IsInRole(userId, GetRoles().Where(r => roleNames.Contains(r.Name)).Select(r => r.Id).ToList());
        }

        public static bool IsInRoleNames(List<string> roleNames, bool throwException = false)
        {
            return IsInRole(GetRoles().Where(r => roleNames.Contains(r.Name)).Select(r => r.Id).ToList(), throwException);
        }

        public static bool IsInRole(List<string> roleIds, bool throwException = false)
        {
            var inRole = false;
            if (Authentication.IsAuthenticated)
                inRole = roleIds.Count == 0 || roleIds.Exists(r => Authentication.AuthenticatedUser.IsInRole(r));
            if (!inRole && throwException)
                throw new Exception(Localization.GetLocalization(LocalizationType.Exception, "AccessDenied.Error", "Access Denied.", "Core"));
            return inRole;
        }

        public static bool IsInRole(string userId, List<string> roleIds)
        {
            var user = Authentication.AuthenticatedUser; //GetUserById(userId);
            if (user != null)
                return user.RoleIds.Exists(r => roleIds.Contains(r));
            return false;
        }

        public static Models.User CurrentUser
        {
            get
            {
                if (Authentication.IsAuthenticated)
                {
                    var user = Portal.GetRequestContextData<Models.User>("VidereCurrentUser", null);
                    if (user == null)
                    {
                        //todo: this could be expensive to do a lookup to the database each time!
                        user = GetUserById(Authentication.AuthenticatedUserId, true);
                        if (user == null)
                            Core.Services.Authentication.RevokeAuthenticationTicket();
                        else 
                            Portal.SetRequestContextData("VidereCurrentUser", user);
                    }
                    return user;
                }
                return null;
            }
        }

        public static Models.User GetUserById(string id, bool clone = false)
        {
            var user = AccountService.GetById(id);
            if (clone)
                return user.JsonClone();
            return user;
        }

        public static Models.User Get(Func<Models.User, bool> statement, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return AccountService.Get(portalId, statement).FirstOrDefault();
        }

        public static Models.User GetUser(string name, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return AccountService.Get(portalId).Where(u => u.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            //return Repository.Current.GetResourceData<Models.User>("User", m => m.Data.PortalId == portalId && m.Data.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase), null);
            //return GetUsers(u => u.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && u.PortalId == portalId).SingleOrDefault(); 
        }

        public static List<Models.User> GetUsers(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return AccountService.Get(portalId);
        }

        public static List<Models.User> GetUsers(Func<Models.User, bool> statement, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return AccountService.Get(portalId, statement);
        }

        public static string ImportUser(string portalId, Models.User user, Dictionary<string, string> idMap, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = GetUser(user.Name, portalId);
            user.PortalId = portalId;
            user.Id = existing != null ? existing.Id : null;
            user.RoleIds = Security.GetNewRoleIds(user.RoleIds, idMap);
            return SaveUser(user, userId);
        }

        public static string SaveUser(Models.User user, string editUserId = null)
        {
            user.PortalId = string.IsNullOrEmpty(user.PortalId) ? Portal.CurrentPortalId : user.PortalId;
            editUserId = string.IsNullOrEmpty(editUserId) ? AuditId : editUserId; //we do not blindly accept what user id they passed in user object.  Enforce that editing current logged in user if no editUserId passed

            AccountService.Validate(user);

            //if password set, we need to remember it and remove it from our user object (which is only in memory - probably not necessary)
            var password = user.Password;
            user.Password = null;

            var userId = AccountService.Save(user, editUserId);

            //if we need to update password, then use the persistance provider
            if (!string.IsNullOrEmpty(password))
            {
                if (Authentication.PersistanceProvider != null)
                {
                    var persistanceResult = Authentication.PersistanceProvider.SaveAuthentication(userId, user.Name, password);
                    if (!persistanceResult.Success)
                        throw new Exception(persistanceResult.Errors.ToJson());
                    Authentication.AssociateAuthenticationToken(user, persistanceResult.Provider, persistanceResult.ProviderUserId);
                }
                else
                    throw new Exception("Cannot persist password if no authentication persistance provider enabled");
            }
            return userId;
        }

        public static bool Exists(Models.User user)
        {
            var existing = GetUsers(u => user.Name == u.Name, user.PortalId);
            return existing.Count > 0 && existing[0].Id != user.Id;
        }

        public static bool DeleteUser(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? AuditId : userId;
            return AccountService.Delete(id, userId);
        }

        public static Models.User Login(string userName, string password, bool persistant, string provider)
        {
            var authResult = Authentication.Login(userName, password, provider);
            if (authResult.Success)
            {
                var user = Authentication.GetUserByAuthenticationToken(authResult.Provider, authResult.ProviderUserId);
                
                //if authenticated but not existant, we want to create one
                if (user == null)
                {
                    user = GetUser(userName);   //TODO:  this is temporary as we won't allow changing of userName yet... to support backwards compat allowing user's without tokens to obtain them
                    if (user == null)
                    {
                        user = new Models.User()
                        {
                            Name = userName,
                            PortalId = Portal.CurrentPortalId
                        };
                    }
                    user.Id = SaveUser(user); //must save before we associate
                    Authentication.AssociateAuthenticationToken(user, authResult.Provider, authResult.ProviderUserId);
                }

                //var user = AccountService.GetById(authResult.ProviderUserId);
                if (user != null)
                {
                    var changes = 0;
                    //todo: optimize?  dictionary or something...
                    if (authResult.Claims != null)
                    {
                        foreach (var claim in authResult.Claims)
                        {
                            if (user.GetClaim(claim.Type, claim.Issuer) == null)
                            {
                                user.Claims.Add(claim);
                                changes++;
                            }
                        }
                    }
                    //user.Claims.AddRange(authResult.Claims);
                    if (authResult.ExtraData != null)
                    {
                        foreach (var key in authResult.ExtraData.Keys)
                        {
                            if (!user.Attributes.ContainsKey(key) || !user.Attributes[key].ToString().Equals(authResult.ExtraData[key], StringComparison.InvariantCultureIgnoreCase))
                            {
                                user.Attributes[key] = authResult.ExtraData[key];
                                changes++;
                            }
                        }
                    }
                    if (changes > 0)
                        SaveUser(user); //for now we will persist any information coming back from the authentication provider...  may change mind

                    Authentication.IssueAuthenticationTicket(user.Id.ToString(), user.RoleIds, 30, persistant); //todo: ticket needs refactoring on who does it
                }
                return user;
            }
            return null;
        }

        public static bool RoleAuthorized(List<string> roles)
        {
            return (roles.Count == 0 || roles.Exists(r => Services.Account.IsInRole(r)));
        }

        public static Models.Role GetRoleById(string id)
        {
            return AccountService.GetRoleById(id);
        }

        public static List<Models.Role> GetRoles(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return AccountService.GetRoles(portalId);
        }

        public static Models.Role GetRole(string name, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return GetRoles(portalId).Where(m => m.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public static string ImportRole(string portalId, Models.Role role, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = GetRole(role.Name, portalId);
            role.PortalId = portalId;
            role.Id = existing != null ? existing.Id : null;
            return SaveRole(role, userId);
        }

        public static string SaveRole(Models.Role role, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? AuditId : userId;
            role.PortalId = string.IsNullOrEmpty(role.PortalId) ? Portal.CurrentPortalId : role.PortalId;

            Validate(role);
            return AccountService.SaveRole(role, userId);
        }

        public static void Validate(Models.Role role)
        {
            if (string.IsNullOrEmpty(role.Name))
                throw new Exception(Localization.GetExceptionText("InvalidResource.Error", "{0} is invalid.", "Role"));
            else if (IsDuplicate(role))
                throw new Exception(Localization.GetExceptionText("DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "Role"));
            else if (IsAdminRole(role.Id) && role.Name != "admin")
                throw new Exception(Localization.GetExceptionText("CannotChangeCoreData.Error", "{0} is part of the core data structure, it is not allowed to be altered.", "admin"));
        }

        public static bool IsDuplicate(Models.Role role)
        {
            var r = GetRole(role.Name, role.PortalId);
            if (r != null)
                return r.Id != role.Id;
            return false;
        }

        public static bool Exists(Models.Role role)
        {
            return GetRole(role.Name, role.PortalId) != null;
        }

        public static bool IsAdminRole(string id)
        {
            var role = GetRoleById(id);
            return role != null && role.Name == "admin";
        }

        public static bool DeleteRole(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? AuditId : userId;
            return AccountService.DeleteRole(id, userId);
        }

        public static Models.UserProfile GetUserProfile(string id = null)
        {
            id = string.IsNullOrEmpty(id) ? Account.CurrentIdentityName : id;
            var user = GetUserById(id);
            if (user != null)
                return new Models.UserProfile(user);
            return null;
        }

        public static List<CustomDataElement> GetEditableProfileElements()
        {
            return Account.CustomUserElements.Where(e => e.UserCanEdit).ToList();
        }

        public static TimeZoneInfo GetUserTimeZone()
        {
            var user = Account.CurrentUser;
            if (user != null && !string.IsNullOrEmpty(user.TimeZone))
                return TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);
            return null;
        }

        public static string GetUserTimeZoneName()
        {
            var zone = GetUserTimeZone();
            if (zone != null)
                return zone.StandardName;
            return "";
        }

        public static bool SaveUserProfile(Models.UserProfile userProfile)
        {
            Validate(userProfile);

            //cloning user so cached dies not get changed until actual save occurs
            var user = GetUserById(userProfile.Id, true);
            userProfile.ApplyProfileToUser(user);
            return !string.IsNullOrEmpty(SaveUser(user, userProfile.Id));   //at the point of calling the Account service we should have already validated user has permission to change this user id
        }

        private static void Validate(Models.UserProfile userProfile)
        {
            if (userProfile.Password1 != userProfile.Password2)
                throw new Exception(Localization.GetExceptionText("InvalidChangePassword.Error", "Passwords don't match."));
        }

    }
}
