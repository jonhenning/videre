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

        public static void RegisterCustomUserElement(Models.CustomDataElement element)
        {
            if (!CustomUserElements.Exists(e => e.Name.Equals(element.Name, StringComparison.InvariantCultureIgnoreCase)))
                CustomUserElements.Add(element);    //todo: worry about locking?
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

        public static string SaveUser(Models.User user, string editUserId = null)
        {
            user.PortalId = string.IsNullOrEmpty(user.PortalId) ? Portal.CurrentPortalId : user.PortalId;
            editUserId = string.IsNullOrEmpty(editUserId) ? AuditId : editUserId; //we do not blindly accept what user id they passed in user object.  Enforce that editing current logged in user if no editUserId passed

            AccountService.Validate(user);

            //if password set, we need to remember it and remove it from our user object (which is only in memory - probably not necessary)
            var password = user.Password;
            user.Password = null;

            var emailChanged = false;
            var userNameChanged = false;
            if (!string.IsNullOrEmpty(user.Id))
            {
                var existingUser = AccountService.GetById(user.Id);
                if (existingUser != null )
                {
                    emailChanged = existingUser.Email != user.Email;
                    userNameChanged = existingUser.Name != user.Name;
                }
            }

            var userId = AccountService.Save(user, editUserId);
            user.Id = userId;

            if (emailChanged && AccountVerificationMode != "None")
                RemoveAccountVerification(user.Id);

            //if we need to update password or username, then use the persistance provider
            if (!string.IsNullOrEmpty(password) || userNameChanged)
            {
                if (Authentication.PersistenceProvider != null)
                {
                    var persistanceResult = Authentication.PersistenceProvider.SaveAuthentication(userId, user.Name, password);
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

        public static bool RoleAuthorized(List<string> roleIds)
        {
            return (roleIds.Count == 0 || roleIds.Exists(r => Services.Account.IsInRole(r)));
        }

        public static bool UserNotInRole(List<string> roleIds)
        {
            return (roleIds.Count == 0 || !roleIds.Exists(r => Services.Account.IsInRole(r)));
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

        public static bool CreateAccount(Models.UserProfile userProfile)
        {
            Validate(userProfile);

            var user = new User();
            userProfile.ApplyProfileToUser(user);
            var userId = SaveUser(user, userProfile.Id);
            if (!string.IsNullOrEmpty(userId))
            {
                if (AccountVerificationMode != "None")
                    IssueAccountVerificationCode(userId);
                return true;
            }
            return false;
            
            //todo:  auto login???
            //if (!string.IsNullOrEmpty(userId))
            //{
                //var loginResult = Authentication.Login(userProfile.Name, userProfile.Password1, false, Authentication.PersistanceProvider.Name);
                //return loginResult.
                
            //}
            //return false;
        }

        public static string AccountVerificationMode
        {
            get
            {
                return Services.Portal.GetPortalAttribute("Authentication", "AccountVerificationMode", "None");
            }
        }

        public static string AccountVerificationUrl { get { return Services.Portal.ResolveUrl(Services.Portal.GetPortalAttribute("Authentication", "AccountVerificationUrl", "~/account/verify")); } }

        public static bool IsAccountVerified(Models.User user)
        {
            var code = user.GetClaimValue("Account Verification Code", "Videre Account Verification", "");
            var verifiedOn = user.GetClaimValue<string>("Account Verified On", "Videre Account Verification", null);
            return !string.IsNullOrEmpty(code) && verifiedOn != null;
        }

        public static bool VerifyAccount(string userId, string verificationCode)
        {
            var user = GetUserById(userId);
            if (user != null)
            {
                if (!IsAccountVerified(user))
                {
                    if (user.GetClaimValue("Account Verification Code", "Videre Account Verification", "") == verificationCode)
                    {
                        user.Claims.Add(new UserClaim() { Type = "Account Verified On", Issuer = "Videre Account Verification", Value = DateTime.UtcNow.ToJson() });
                        Account.SaveUser(user);
                        return true;
                    }
                }
                else
                    return true;    //already verified
            }
            return false;
        }

        public static bool RemoveAccountVerification(string userId)
        {
            var user = GetUserById(userId);
            if (user != null)
            {
                var claims = user.Claims.Where(c => c.Issuer == "Videre Account Verification").ToList();
                foreach (var claim in claims)
                    user.Claims.Remove(claim);
                if (claims.Count > 0)
                {
                    Account.SaveUser(user);
                    return true;
                }
            }
            return false;
        }

        public static bool IssueAccountVerificationCode(string userId)
        {
            var user = GetUserById(userId);
            var claim = user.GetClaim("Account Verification Code", "Videre Account Verification");
            if (claim == null)
            {
                claim = new UserClaim() { Type = "Account Verification Code", Issuer = "Videre Account Verification", Value = System.Web.Security.Membership.GeneratePassword(8, 2) };
                user.Claims.Add(claim);
                Account.SaveUser(user);
            }
            sendVerificationCode(user, claim.Value);
            return true;
        }

        private static void sendVerificationCode(Models.User user, string code)
        {
            var subject = Services.Localization.GetPortalText("PortalEmailAccountVerificationSubject.Text", "Account Verification Code");
            var body = Services.Localization.GetPortalText("PortalEmailAccountVerificationBody.Text", "<p>Please verify your account by logging into <a href=\"$Url\">your account</a> and entering the following code when asked.</p><p><b><a href=\"$Url\">$Code</a></b></p>");
            var tokens = new Dictionary<string, object>()
                {
                    {"Code", code},
                    {"Url", Portal.RequestRootUrl.PathCombine(Account.AccountVerificationUrl) + "?code=" + HttpUtility.UrlEncode(code)}
                };
            if (!string.IsNullOrEmpty(Services.Portal.CurrentPortal.AdministratorEmail))
                Services.Mail.Send(Services.Portal.CurrentPortal.AdministratorEmail, user.Email, "AccountVerification", subject, body, tokens, true);
            else
                throw new Exception(Services.Localization.GetExceptionText("AdministratorEmailNotSet.Text", "Administrator Email not set.  Please contact the portal administrator."));
        }

        private static void Validate(Models.UserProfile userProfile)
        {
            if (userProfile.Password1 != userProfile.Password2)
                throw new Exception(Localization.GetExceptionText("InvalidChangePassword.Error", "Passwords don't match."));
        }

    }
}
