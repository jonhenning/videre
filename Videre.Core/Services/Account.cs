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
        private static IAccountService _accountService;

        private static IAccountService AccountService
        {
            get
            {
                if (_accountService == null)
                {
                    //ObjectFactory.Configure(x =>
                    //    x.Scan(scan =>
                    //    {
                    //        scan.Assembly(ConfigurationManager.AppSettings.GetSetting("AccountServicesProvider", "Videre.Core"));
                    //        scan.AddAllTypesOf<IAccountService>();
                    //    }));

                    //_accountService = ObjectFactory.GetInstance<IAccountService>();
                    _accountService = ConfigurationManager.AppSettings.GetSetting("AccountServicesProvider", "Videre.Core.Services.DefaultAccount, Videre.Core").GetInstance<IAccountService>();
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

        public static bool ReadOnly { get { return AccountService.ReadOnly; } } //todo: kinda hacky...

        public static bool IsAuthenticated
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.User != null)
                    return HttpContext.Current.User.Identity.IsAuthenticated;
                return false;
            }
        }

        public static void VerifyInRole(string role)
        {
            IsInRole(new List<string>() { role }, true);
        }

        public static void VerifyInRole(List<string> roles)
        {
            IsInRole(roles, true);
        }

        public static bool IsInRole(string role, bool throwException = false)
        {
            return IsInRole(new List<string>() { role }, throwException);
        }

        public static bool IsInRoleNames(List<string> roles, bool throwException = false)
        {
            return IsInRole(GetRoles().Where(r => roles.Contains(r.Name)).Select(r => r.Id).ToList(), throwException);
        }

        public static bool IsInRole(List<string> roleIds, bool throwException = false)
        {
            var inRole = false;
            if (IsAuthenticated)
                inRole = roleIds.Count == 0 || roleIds.Exists(r => HttpContext.Current.User.IsInRole(r));
            if (!inRole && throwException)
                throw new Exception(Localization.GetLocalization(LocalizationType.Exception, "AccessDenied.Error", "Access Denied.", "Core"));
            return inRole;
        }

        public static string CurrentIdentityName
        {
            get
            {
                if (IsAuthenticated)
                    return HttpContext.Current.User.Identity.Name;
                return null;
            }
        }

        public static string AuditId
        {
            get
            {
                if (CurrentUser != null)
                    return CurrentUser.Id;
                return null;
            }
        }

        public static Models.User CurrentUser
        {
            get
            {
                if (IsAuthenticated)
                {
                    var user = GetUserById(CurrentIdentityName);
                    if (user == null)
                        RevokeAuthenticationTicket();
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

        public static Models.User GetUser(string name, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return Repository.Current.GetResourceData<Models.User>("User", m => m.Data.PortalId == portalId && m.Data.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase), null);
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
            user.Roles = Security.GetNewRoleIds(user.Roles, idMap);
            return SaveUser(user, userId);
        }

        public static string SaveUser(Models.User user, string editUserId = null)
        {
            user.PortalId = string.IsNullOrEmpty(user.PortalId) ? Portal.CurrentPortalId : user.PortalId;
            editUserId = string.IsNullOrEmpty(editUserId) ? AuditId : editUserId; //we do not blindly accept what user id they passed in user object.  Enforce that editing current logged in user if no editUserId passed

            if (string.IsNullOrEmpty(user.Password))
            {
                var existing = GetUserById(user.Id);
                if (existing != null)
                {
                    user.PasswordHash = existing.PasswordHash;  //not sending hash to client, so we need to get it from server.
                    user.PasswordSalt = existing.PasswordSalt;
                }
            }

            Validate(user);
            return AccountService.Save(user, editUserId);
        }

        public static void Validate(Models.User user)
        {
            Validation.ValidateEmail(user.Email);
            if (string.IsNullOrEmpty(user.Name) || (string.IsNullOrEmpty(user.Password) && string.IsNullOrEmpty(user.PasswordHash)))
                throw new Exception(Localization.GetExceptionText("InvalidResource.Error", "{0} is invalid.", "User"));
            if (Exists(user))
                throw new Exception(Localization.GetExceptionText("DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "User"));
        }

        public static bool Exists(Models.User user)
        {
            var existing = GetUsers(u => user.Name == u.Name, user.PortalId);
            return existing.Count > 0 && existing[0].Id != user.Id;
        }

        public static bool DeleteUser(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? AuditId :  userId;
            return AccountService.Delete(id, userId);
        }

        public static Models.User Login(string UserName, string Password, bool Persistant)
        {
            var user = AccountService.Login(UserName, Password);
            if (user != null)
            {
                IssueAuthenticationTicket(user.Id.ToString(), user.Roles, 30, Persistant);
            }
            return user;
        }

        public static void IssueAuthenticationTicket(string IdentityName, List<string> Roles, int Days, bool Persistant)
        {
            var ticket = new FormsAuthenticationTicket(1, IdentityName, DateTime.Now, DateTime.Now.AddDays(Days), Persistant, String.Join("," , Roles));
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            cookie.Expires = ticket.Expiration;
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static void RevokeAuthenticationTicket()
        {
            FormsAuthentication.SignOut();
        }

        public static bool RoleAuthorized(List<string> Roles)
        {
            return (Roles.Count == 0 || Roles.Exists(r => Services.Account.IsInRole(r)));
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
            userId = string.IsNullOrEmpty(userId) ? AuditId :  userId;
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
            userId = string.IsNullOrEmpty(userId) ? AuditId :  userId;
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

        public static bool SaveUserProfile(Models.UserProfile userProfile)
        {
            Validate(userProfile);

            //cloning user so cached dies not get changed until actual save occurs
            var user = GetUserById(userProfile.Id, true);

            //todo: mapping here or in object?
            user.Name = userProfile.Name;
            user.Email = userProfile.Email;
            user.Locale = userProfile.Locale;
            user.Password = userProfile.Password1;
            return !string.IsNullOrEmpty(SaveUser(user, userProfile.Id));   //at the point of calling the Account service we should have already validated user has permission to change this user id
        }

        private static void Validate(Models.UserProfile userProfile)
        {
            if (userProfile.Password1 != userProfile.Password2)
                throw new Exception(Localization.GetExceptionText("InvalidChangePassword.Error", "Passwords don't match."));
        }

    }
}
