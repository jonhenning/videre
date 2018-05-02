using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using Videre.Core.Models;
using CodeEndeavors.Extensions;
using System.Configuration;
using System.Security.Cryptography;
using System.Globalization;

namespace Videre.Core.Services
{
    public class Account
    {
        private static AccountProviders.IAccountService _accountService;

        private static List<Providers.IVerifyAccountProvider> _verifyAuthenticationProviders = null;

        public static List<Providers.IVerifyAccountProvider> VerifyAuthenticationProviders
        {
            get
            {
                if (_verifyAuthenticationProviders == null)
                    _verifyAuthenticationProviders = ReflectionExtensions.GetAllInstances<Providers.IVerifyAccountProvider>();
                return _verifyAuthenticationProviders;
            }
        }

        public static Providers.IVerifyAccountProvider VerifyAuthenticationProvider
        {
            get
            {
                return VerifyAuthenticationProviders.Where(p => p.Name == Services.Portal.GetPortalSetting("Account", "AccountVerificationProvider", "Videre")).FirstOrDefault();
            }
        }


        public static AccountProviders.IAccountService AccountService
        {
            get
            {
                if (_accountService == null)
                {
                    //_accountService = Portal.GetPortalAttribute("Account", "AccountProvider", Portal.GetAppSetting("AccountServicesProvider", "Videre.Core.AccountProviders.VidereAccount, Videre.Core")).GetInstance<AccountProviders.IAccountService>();
                    _accountService = Portal.GetAppSetting("AccountServicesProvider", "Videre.Core.AccountProviders.VidereAccount, Videre.Core").GetInstance<AccountProviders.IAccountService>();
                    _accountService.Initialize(Portal.GetAppSetting("AccountServicesConnection", ""));
                }
                return _accountService;
            }
        }

        public static bool TwoPhaseAuthenticationEnabled
        {
            get
            {
                return AccountVerificationMode != "None" || UserTwoPhaseAuthenticationEnabled;
            }
        }

        public static bool UserTwoPhaseAuthenticationEnabled
        {
            get
            {
                return Services.Portal.GetPortalAttribute("Authentication", "EnableTwoPhaseAuthentication", "No") == "Yes";
            }
        }

        public static List<Models.CustomDataElement> CustomUserElements
        {
            get
            {
                var elements = AccountService.CustomUserElements;

                if (UserTwoPhaseAuthenticationEnabled)
                {
                    //this seems a bit hacky to accomplish this here like this
                    if (!elements.Exists(e => e.Name == "Enable Two-Phase Authentication"))
                    {
                        elements.Add(new CustomDataElement()
                            {
                                Name = "Enable Two-Phase Authentication",
                                DataType = typeof(Boolean),
                                UserCanEdit = true
                            });
                    }
                }
                return elements;
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

        [Obsolete]
        public static void VerifyInRole(string roleId)
        {
            IsInRole(new List<string>() { roleId }, true);
        }

        [Obsolete]
        public static void VerifyInRole(List<string> roleIds)
        {
            IsInRole(roleIds, true);
        }

        [Obsolete]
        public static bool IsInRole(string userId, string roleId)
        {
            return IsInRole(userId, new List<string>() { roleId });
        }

        [Obsolete]
        public static bool IsInRole(string roleId, bool throwException = false)
        {
            return IsInRole(new List<string>() { roleId }, throwException);
        }

        [Obsolete]
        public static bool IsInRoleNames(string userId, List<string> roleNames)
        {
            return IsInRole(userId, GetRoles().Where(r => roleNames.Contains(r.Name)).Select(r => r.Id).ToList());
        }

        [Obsolete]
        public static bool IsInRoleNames(List<string> roleNames, bool throwException = false)
        {
            return IsInRole(GetRoles().Where(r => roleNames.Contains(r.Name)).Select(r => r.Id).ToList(), throwException);
        }

        [Obsolete]
        public static bool IsInRole(List<string> roleIds, bool throwException = false)
        {
            var inRole = false;
            if (Authentication.IsAuthenticated)
                inRole = roleIds.Count == 0 || roleIds.Exists(r => Authentication.AuthenticatedUser.IsInRole(r));
            if (!inRole && throwException)
                throw new Exception(Localization.GetLocalization(LocalizationType.Exception, "AccessDenied.Error", "Access Denied.", "Core"));
            return inRole;
        }

        [Obsolete]
        public static bool IsInRole(string userId, List<string> roleIds)
        {
            //var user = Authentication.AuthenticatedUser; //GetUserById(userId);
            var user = Account.GetUserById(userId);
            if (user != null)
                return user.RoleIds.Exists(r => roleIds.Contains(r));
            return false;
        }

        [Obsolete]
        public static bool IsInClaim(string issuer, string type, string value)
        {
            return IsInClaim(new List<Models.UserClaim>() { new Models.UserClaim() { Issuer = issuer, Type = type, Value = value } });
        }

        [Obsolete]
        public static bool IsInClaim(Models.UserClaim claim)
        {
            return IsInClaim(new List<Models.UserClaim>() { claim });
        }

        [Obsolete]
        public static bool IsInClaim(List<UserClaim> claims, bool throwException = false)
        {
            var inClaim = false;
            if (Authentication.IsAuthenticated)
                inClaim = claims.Count == 0 || claims.Exists(c => Authentication.AuthenticatedUser.HasClaim(c));
            if (!inClaim && throwException)
                throw new Exception(Localization.GetLocalization(LocalizationType.Exception, "AccessDenied.Error", "Access Denied.", "Core"));
            return inClaim;
        }

        [Obsolete]
        public static bool RoleAuthorized(List<string> roleIds)
        {
            return (roleIds.Count == 0 || roleIds.Exists(r => Services.Account.IsInRole(r)));
        }

        [Obsolete]
        public static bool UserNotInRole(List<string> roleIds)
        {
            return (roleIds.Count == 0 || !roleIds.Exists(r => Services.Account.IsInRole(r)));
        }

        public static Models.User CurrentUser
        {
            get
            {
                if (Authentication.IsAuthenticated)
                {
                    var user = GetUserById(Authentication.AuthenticatedUserId, true);
                    if (user == null)
                        Core.Services.Authentication.RevokeAuthenticationTicket();
                    return user;
                }
                return null;
            }
        }

        public static Models.User CurrentNonImpersonatedUser
        {
            get
            {
                if (Authentication.IsAuthenticated)
                {
                    var user = GetUserById(Authentication.NonImpersonatedAuthenticatedUser.Id, true);
                    if (user == null)
                        Core.Services.Authentication.RevokeAuthenticationTicket();
                    return user;
                }
                return null;
            }
        }

        public static Models.User GetUserById(string id, bool clone = false)
        {
            var user = Portal.GetRequestContextData<Models.User>("VidereUserRequestCache-" + id, null);
            if (user == null)
            {
                user = AccountService.GetById(id);  //todo: this could be expensive to do a lookup to the database each time!

                if (clone)  //we are ok with cloning only once inside requestcache...  
                    user = user.JsonClone();
                Portal.SetRequestContextData("VidereUserRequestCache-" + id, user);
            }
            return user;
        }

        public static Models.User Get(Func<Models.User, bool> statement, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return AccountService.Get(portalId, statement).FirstOrDefault();
        }

        public static Models.User GetUserByClaim(string issuer, string type, string value, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            if (AccountService is AccountProviders.IClaimsAccountService)
                return ((AccountProviders.IClaimsAccountService)AccountService).GetByClaim(portalId, issuer, type, value);

            return Get(u => u.Claims.Exists(c => c.Issuer == issuer && c.Type == type && c.Value == value), portalId);
        }

        public static Models.User GetUser(string name, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            if (AccountService is AccountProviders.IClaimsAccountService)
                return ((AccountProviders.IClaimsAccountService)AccountService).GetByName(portalId, name);
            return AccountService.Get(portalId).Where(u => u.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
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
                if (existingUser != null)
                {
                    emailChanged = existingUser.Email != user.Email;
                    userNameChanged = existingUser.Name != user.Name;
                }
            }

            if (userNameChanged)
            {
                var existing = GetUser(user.Name);
                if (existing != null)
                    throw new Exception("Username already exists");
            }

            var userId = AccountService.Save(user, editUserId);
            user.Id = userId;

            if (emailChanged && AccountVerificationMode != "None")
                RemoveAccountVerification(user.Id);

            //if we need to update password or username, then use the persistence provider
            if (!string.IsNullOrEmpty(password) || userNameChanged)
            {
                if (Authentication.PersistenceProvider != null)
                {
                    var persistenceResult = Authentication.SaveUserAuthentication(userId, user.Name, password);
                    if (!persistenceResult.Success)
                        throw new Exception(persistenceResult.Errors.ToJson());
                    Authentication.AssociateAuthenticationToken(user, persistenceResult.Provider, persistenceResult.ProviderUserId);
                }
                else
                    throw new Exception("Cannot persist password if no authentication persistence provider enabled");
            }
            Portal.SetRequestContextData<Models.User>("VidereUserRequestCache-" + userId, null);    //clear record from context 

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
            var auths = Authentication.PersistenceProvider.GetUserAuthentications(id);
            if (auths != null)
            {
                foreach (var auth in auths)
                    Authentication.DeleteUserAuthentication(auth.Id, userId);
            }

            return AccountService.Delete(id, userId);
        }

        //public static bool RoleOrClaimAuthorized(List<string> roleIds, List<UserClaim> claims)
        //{
        //    roleIds = roleIds ?? new List<string>();
        //    claims = claims ?? new List<UserClaim>();
        //    return ((roleIds.Count == 0 && claims.Count == 0) ||
        //        (roleIds.Exists(r => Services.Account.IsInRole(r)) || claims.Exists(c => Services.Account.IsInClaim(c))));
        //}

        public static Models.Role GetRoleById(string id)
        {
            return AccountService.GetRoleById(id);
        }

        public static List<Models.Role> GetRoles(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            var roles = Portal.GetRequestContextData<List<Models.Role>>("PortalRoles-" + portalId, null);
            if (roles == null)
            {
                roles = AccountService.GetRoles(portalId);
                Portal.SetRequestContextData("PortalRoles-" + portalId, roles);
            }
            return roles;
        }

        public static List<Models.UserClaim> GetClaims(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            if (AccountService is AccountProviders.IClaimsAccountService)
                return ((AccountProviders.IClaimsAccountService)AccountService).GetClaims(portalId);
            else
                return new List<UserClaim>();
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
            return GetUserTimeZone(Account.CurrentUser);
        }

        public static TimeZoneInfo GetUserTimeZone(Models.User user)
        {
            if (user != null && !string.IsNullOrEmpty(user.TimeZone))
                return TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);
            return null;
        }

        public static string GetUserTimeZoneName()
        {
            return GetUserTimeZoneName(Account.CurrentUser);
        }

        public static string GetUserTimeZoneName(Models.User user)
        {
            var zone = GetUserTimeZone(user);
            if (zone != null)
                return zone.Id; //using Id for Name
            return "";
        }

        public static Models.NumberFormat GetUserNumberFormat()
        {
            return GetUserNumberFormat(Services.Account.CurrentUser);
        }
        public static Models.NumberFormat GetUserNumberFormat(Models.User user, bool returnDefault = true)
        {
            if (user != null && !string.IsNullOrEmpty(user.Locale))
            {
                var culture = new CultureInfo(user.Locale);
                return new Models.NumberFormat()
                {
                    CurrencyDecimalDigits = culture.NumberFormat.CurrencyDecimalDigits,
                    CurrencyDecimalSeparator = culture.NumberFormat.CurrencyDecimalSeparator,
                    CurrencyGroupSeparator = culture.NumberFormat.CurrencyGroupSeparator,
                    CurrencySymbol = culture.NumberFormat.CurrencySymbol
                };
            }
            else if (returnDefault)
            {
                var culture = new CultureInfo("en-US");
                return new Models.NumberFormat()
                {
                    CurrencyDecimalDigits = culture.NumberFormat.CurrencyDecimalDigits,
                    CurrencyDecimalSeparator = culture.NumberFormat.CurrencyDecimalSeparator,
                    CurrencyGroupSeparator = culture.NumberFormat.CurrencyGroupSeparator,
                    CurrencySymbol = culture.NumberFormat.CurrencySymbol
                };
            }
            return null;
        }

        /// <summary>
        /// Returns logged in users date format
        /// </summary>
        /// <param name="returnDefault">determins if no format specified for user if the defaults are returned</param>
        /// <returns></returns>
        public static string GetUserDateFormat(string format, bool returnDefault = true)
        {
            return GetUserDateFormat(Services.Account.CurrentUser, format, returnDefault);
        }

        public static string GetUserDateFormat(Models.User user, string format, bool returnDefault = true)
        {
            if (user != null && !string.IsNullOrEmpty(user.Locale))
            {
                var culture = new CultureInfo(user.Locale);
                if (format == "date")
                    return "L"; //convertDateFormatToMomentJS(culture.DateTimeFormat.ShortDatePattern);
                else if (format == "datetime")
                    return "L " + convertDateFormatToMomentJS(culture.DateTimeFormat.ShortTimePattern); //"L LT";//convertDateFormatToMomentJS(culture.DateTimeFormat.ShortDatePattern + " " + culture.DateTimeFormat.ShortTimePattern);
                else if (format == "time")
                    return convertDateFormatToMomentJS(culture.DateTimeFormat.ShortTimePattern); //"LT";//convertDateFormatToMomentJS(culture.DateTimeFormat.ShortTimePattern);
                else
                    throw new Exception("Unknown Date Format: " + format);
            }
            else if (returnDefault)
            {
                if (format == "date")
                    return "M/D/YY";
                else if (format == "datetime")
                    return "M/D/YY h:mm A";
                else if (format == "time")
                    return "h:mm A";
                else
                    throw new Exception("Unknown Date Format: " + format);
            }
            return null;
        }

        private static string convertDateFormatToMomentJS(string format)
        {
            //=======================================================================   
            // Handle:   
            //  C#     Moment  Meaning   
            //  d      D       Day of month (no leading 0)  
            //  dd     DD      Day of month (leading 0)   
            //  M      M       Month of year (no leading 0)   
            //  MM     MM      Month of year (leading 0)   
            //  yy     Y       Two digit year   
            //  yyyy   YY      Four digit year 
            //  tt     A       AM/PM
            //======================================================================= 

            //not really elegant...   
            format = format.Replace("dd", "DD");
            format = format.Replace("d", "D");
            format = format.Replace("yyyy", "YY");
            format = format.Replace("yy", "Y");
            format = format.Replace("tt", "A");

            return format;
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

            var existing = GetUser(userProfile.Name);
            if (existing != null)
                throw new Exception("Username already exists");

            var user = new User();
            userProfile.ApplyProfileToUser(user);
            var userId = SaveUser(user, userProfile.Id);
            if (!string.IsNullOrEmpty(userId))
            {
                if (AccountVerificationMode != "None")
                    IssueAccountVerificationCode(userId);

                //auto-login - assume no must change password, skip verification on this one, 
                var loginResult = Authentication.Login(userProfile.Name, userProfile.Password1, false, Authentication.PersistenceProvider.Name);
                return !string.IsNullOrEmpty(loginResult.UserId);
            }
            return false;
        }

        public static string AccountVerificationMode
        {
            get
            {
                return Services.Portal.GetPortalAttribute("Account", "AccountVerificationMode", "None");
            }
        }

        public static string AccountVerificationUrl { get { return Services.Portal.ResolveUrl(Services.Portal.GetPortalAttribute("Account", "AccountVerificationUrl", "~/account/verify")); } }

        //[Obsolete("Use user.IsEmailVerified")]
        public static bool IsAccountVerified(IAuthorizationUser user)
        {
            if (VerifyAuthenticationProvider != null)
                return VerifyAuthenticationProvider.IsAccountVerified(user);
            return false;
        }

        public static bool VerifyAccount(string userId, string verificationCode)
        {
            return VerifyAccount(userId, verificationCode, false);
        }

        public static bool VerifyAccount(string userId, string verificationCode, bool trust)
        {
            var ret = false;
            if (VerifyAuthenticationProvider != null)
            {
                var user = GetUserById(userId);
                if (user != null)
                {
                    if (VerifyAuthenticationProvider.VerifyAccount(user, verificationCode, trust))
                    {
                        Account.SaveUser(user);
                        Authentication.UpdateAuthenticationTicketPrincipal();
                        ret = true;
                    }
                }
            }
            return ret;
        }

        //allow for admin verification
        public static bool VerifyAccount(string userId)
        {
            if (VerifyAuthenticationProvider != null)
            {
                var user = GetUserById(userId);
                if (VerifyAuthenticationProvider.ForceVerifyAccount(user))  //true if needed save
                    Account.SaveUser(user);
                return true;
            }
            return false;
        }

        public static bool RemoveAccountVerification(string userId)
        {
            if (VerifyAuthenticationProvider != null)
            {
                var user = GetUserById(userId);
                if (user != null)
                {
                    if (VerifyAuthenticationProvider.RemoveAccountVerification(user))
                        Account.SaveUser(user);
                    return true;
                }
            }
            return false;
        }

        public static bool IssueAccountVerificationCode(string userId)
        {
            if (VerifyAuthenticationProvider != null)
            {
                var code = VerifyAuthenticationProvider.GenerateVerificationCode();
                var user = GetUserById(userId);
                if (VerifyAuthenticationProvider.IssueAccountVerificationCode(user, code))
                    Account.SaveUser(user);
                VerifyAuthenticationProvider.SendVerification(user, code);
                return true;
            }
            return false;
        }

        public static bool UserRequiresTwoPhaseVerification(string id = null)
        {
            if (Account.UserTwoPhaseAuthenticationEnabled)
            {
                Models.User user = null;
                if (!string.IsNullOrEmpty(id))
                    user = GetUserById(id);
                else
                    user = CurrentNonImpersonatedUser;
                if (user != null)
                    return UserRequiresTwoPhaseVerification(user);
            }
            return false;
        }

        public static bool UserRequiresTwoPhaseVerification(Models.User user)
        {
            if (Account.UserTwoPhaseAuthenticationEnabled)
                return user.Attributes.GetSetting("Enable Two-Phase Authentication", false);
            return false;
        }

        private static void Validate(Models.UserProfile userProfile)
        {
            if (userProfile.Password1 != userProfile.Password2)
                throw new Exception(Localization.GetExceptionText("InvalidChangePassword.Error", "Passwords don't match."));

            if (!string.IsNullOrEmpty(userProfile.Locale))
            {
                if (!CultureInfo.GetCultures(CultureTypes.AllCultures).Any(culture => culture.Name.Equals(userProfile.Locale, StringComparison.InvariantCultureIgnoreCase)))
                    throw new Exception(Localization.GetExceptionText("InvalidLocaleCode.Error", "Invalid locale code."));
            }

        }

    }
}
