using CodeEndeavors.Extensions;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Videre.Core.Models;
using Videre.Core.Providers;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Services
{

    public class LoginResult
    {
        public string UserId { get; set; }
        public bool MustChangePassword { get; set; }
        public bool MustVerify { get; set; }
        public string RedirectUrl { get; set; }
    }

    public class AuthenticationResult
    {
        public AuthenticationResult()
        {
            Errors = new List<string>();
            Claims = new List<CoreModels.UserClaim>();
            Roles = new List<string>();
        }

        public bool Success { get; set; }
        public List<string> Errors { get; set; }
        public string Provider { get; set; }
        public string ProviderUserId { get; set; }
        public string UserName { get; set; }
        public List<Models.UserClaim> Claims { get; set; }
        public List<string> Roles { get; set; }
        public IDictionary<string, string> ExtraData { get; set; }
    }

    public class AuthenticationResetResult
    {
        public AuthenticationResetResult()
        {
            Errors = new List<string>();
        }

        public bool Success { get; set; }
        public bool Authenticated { get; set; }
        public List<string> Errors { get; set; }
        public string Provider { get; set; }
        public Models.AuthenticationResetTicket Ticket { get; set; }
        public IDictionary<string, string> ExtraData { get; set; }
    }

    public class Authentication
    {
        private const string _authenticationClaimType = "AuthenticationToken";
        private const string _authenticationAccountNameClaimType = "AuthenticationAccountName";
        private static List<IAuthenticationProvider> _authenticationProviders = new List<IAuthenticationProvider>();
        private static List<IAuthenticationPersistence> _authenticationPersistenceProviders = null;

        private static List<IAuthenticationResetProvider> _authenticationResetProviders = new List<IAuthenticationResetProvider>();

        public static string LoginUrl { get { return FormsAuthentication.LoginUrl; } }

        public static bool IsAuthenticated
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.User != null)
                    return HttpContext.Current.User.Identity.IsAuthenticated;
                return false;
            }
        }

        public static void IssueAuthenticationTicket(string identityName, List<string> roles, int days, bool persistant)
        {
            var ticket = new FormsAuthenticationTicket(1, identityName, DateTime.Now, DateTime.Now.AddDays(days), persistant, String.Join(",", roles));
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            cookie.Expires = ticket.Expiration;
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static void ProcessAuthenticationTicket()
        {
            //' Fires upon attempting to authenticate the use
            if (HttpContext.Current.User != null)
            {
                var identity = (FormsIdentity)HttpContext.Current.User.Identity;
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                    HttpContext.Current.User = new GenericPrincipal(identity, identity.Ticket.UserData.Split(','));
            }
        }

        public static string AuthenticatedUserId
        {
            get
            {
                if (IsAuthenticated)
                    return AuthenticatedUser.Id;
                return null;
            }
        }


        public static Models.AuthenticatedUser AuthenticatedUser
        {
            get
            {
                if (IsAuthenticated)
                {
                    var user = Portal.GetRequestContextData<Models.AuthenticatedUser>("VidereAuthenticatedUser", null);
                    if (user == null)
                    {
                        var principal = (GenericPrincipal)HttpContext.Current.User;
                        var identity = (FormsIdentity)principal.Identity;
                        //eventually add claims when upgrade to .NET 4.5
                        user = new Models.AuthenticatedUser()
                        {
                            Id = identity.Name,
                            RoleIds = identity.Ticket.UserData.Split(',').ToList()
                        };
                        Portal.SetRequestContextData("VidereAuthenticatedUser", user);
                    }
                    return user;

                }
                return null;
            }
        }

        public static void RevokeAuthenticationTicket()
        {
            FormsAuthentication.SignOut();
        }

        public static void RegisterAuthenticationProviders()
        {
            ObjectFactory.Configure(x =>
                x.Scan(scan =>
                {
                    scan.AssembliesFromApplicationBaseDirectory();
                    scan.AddAllTypesOf<IAuthenticationProvider>();
                }));
            _authenticationProviders = ObjectFactory.GetAllInstances<IAuthenticationProvider>().ToList();
            foreach (var provider in _authenticationProviders)
                provider.Register();

        }

        private static Providers.IAuthenticationPersistence _authenticationPersistenceProvider;
        public static Providers.IAuthenticationPersistence PersistenceProvider
        {
            get
            {
                var providerName = Portal.GetAppSetting<string>("AuthenticationPersistenceProvider", null); //Portal.GetPortalAttribute("Authentication", "PersistenceProvider", Portal.GetAppSetting<string>("AuthenticationPersistenceProvider", null));
                if (providerName == null)
                    providerName = "Videre.Core.Providers.VidereAuthenticationProvider, Videre.Core";

                if (_authenticationPersistenceProvider == null && providerName != "")
                {
                    _authenticationPersistenceProvider = providerName.GetInstance<Providers.IAuthenticationPersistence>();
                    _authenticationPersistenceProvider.InitializePersistence(Portal.GetAppSetting("AuthenticationPersistenceConnection", ""));
                }
                return _authenticationPersistenceProvider;
            }
        }

        public static List<IAuthenticationPersistence> GetAuthenticationPersistenceProviders()
        {
            if (_authenticationPersistenceProviders == null)
            {
                ObjectFactory.Configure(x =>
                    x.Scan(scan =>
                    {
                        scan.AssembliesFromApplicationBaseDirectory();
                        scan.AddAllTypesOf<IAuthenticationPersistence>();
                    }));
                _authenticationPersistenceProviders = ObjectFactory.GetAllInstances<IAuthenticationPersistence>().ToList();
            }

            return _authenticationPersistenceProviders;
        }

        public static List<IAuthenticationProvider> GetAuthenticationProviders()
        {
            return _authenticationProviders;
        }

        public static List<IAuthenticationProvider> GetExternalAuthenticationProviders()
        {
            return GetAuthenticationProviders().Where(p => p is IAuthenticationPersistence == false).Select(p => (IAuthenticationProvider)p).ToList();
        }

        public static List<IOAuthAuthenticationProvider> GetOAuthAuthenticationProviders()
        {
            return GetAuthenticationProviders().Where(p => p is IOAuthAuthenticationProvider).Select(p => (IOAuthAuthenticationProvider)p).ToList();
        }

        public static List<IStandardAuthenticationProvider> GetStandardAuthenticationProviders()
        {
            return GetAuthenticationProviders().Where(p => p is IStandardAuthenticationProvider).Select(p => (IStandardAuthenticationProvider)p).ToList();
        }

        public static IAuthenticationProvider GetAuthenticationProvider(string provider)
        {
            return GetAuthenticationProviders().Where(p => p.Name.Equals(provider, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public static Dictionary<string, string> GetUserAuthenticationProviders(CoreModels.User user)
        {
            return user.Claims.Where(c => c.Type == _authenticationClaimType).Select(c => new { Issuer = c.Issuer, AccountName = user.GetClaimValue(_authenticationAccountNameClaimType, c.Issuer, "") }).ToDictionary(c => c.Issuer.ToLower(), c => c.AccountName);
        }

        public static CoreModels.User GetUserByAuthenticationToken(string provider, string token, string portalId = null)
        {
            return CoreServices.Account.Get(u => u.Claims.Exists(c => c.Type == _authenticationClaimType && c.Issuer == provider && c.Value == token), portalId);
        }

        public static CoreModels.User AssociateAuthenticationToken(Models.User user, string providerName, string token)
        {
            //var user = CoreServices.Account.GetUserById(userId);
            var existing = GetUserByAuthenticationToken(providerName, token, user.PortalId);
            var provider = GetAuthenticationProvider(providerName);
            if (existing == null || existing.Id == user.Id || (provider != null && provider.AllowDuplicateAssociation))
            {
                setClaimValue(user, _authenticationClaimType, providerName, token);
                CoreServices.Account.SaveUser(user);
                return user;
            }
            else
                throw new Exception("Authentication Token already associated with another user");
        }

        private static bool setClaimValue(Models.User user, string type, string issuer, string value)
        {
            var changed = false;
            var claim = user.GetClaim(type, issuer);
            if (claim == null)
            {
                claim = new CoreModels.UserClaim() { Issuer = issuer, Type = type };
                user.Claims.Add(claim);
                changed = true;
            }
            else
                changed = claim.Value != value;
            claim.Value = value;
            return changed;
        }

        public static CoreModels.User DisassociateAuthenticationToken(string userId, string provider)
        {
            var user = CoreServices.Account.GetUserById(userId);
            if (user.Claims.Exists(c => c.Type == _authenticationClaimType))
            {
                var authCount = user.Claims.Where(c => c.Type == _authenticationClaimType).Count();
                if (!string.IsNullOrEmpty(user.PasswordHash) || authCount > 1)
                {
                    user.Claims.RemoveAll(c => provider.Equals(c.Issuer, StringComparison.InvariantCultureIgnoreCase));
                    var issuerRoleIds = Account.GetRoles().Where(r => provider.Equals(r.Issuer, StringComparison.InvariantCultureIgnoreCase)).Select(r => r.Id).ToList();
                    user.RoleIds.RemoveAll(id => issuerRoleIds.Contains(id));
                    CoreServices.Account.SaveUser(user);
                    return user;
                }
                else
                    throw new Exception("User cannot remove last form of credentials");
            }
            else
                throw new Exception("User not associated with Authentication Token");
        }

        public static bool AssociateExternalLogin(string associateToUserId, string userName, string password, string provider)
        {
            var authResult = Authentication.Authenticate(userName, password, provider);
            if (authResult.Success)
            {
                var user = Account.GetUserById(associateToUserId);
                if (user != null && !Authentication.GetUserAuthenticationProviders(user).ContainsKey(provider.ToLower()))  //if we have a user and haven't already associated a login token
                {
                    Authentication.AssociateAuthenticationToken(user, authResult.Provider, authResult.ProviderUserId);
                    applyAuthenticationResultToUser(authResult, user);
                    return true;
                }
            }
            else
                throw new Exception("External Login Failed");   //todo: localize?
            return false;
        }

        //needs to be exactly the same between OAuthLogin and ExternLoginCallback methods    - hacky that the service knows URL
        public static string GetOAuthLoginCallbackUrl(string provider, string returnUrl, bool associate)
        {
            return CoreServices.Portal.RequestRootUrl.PathCombine(CoreServices.Portal.ResolveUrl(string.Format("~/core/Account/OAuthLogInCallback?provider={0}&returnUrl={1}&associate={2}", System.Web.HttpUtility.UrlEncode(provider), System.Web.HttpUtility.UrlEncode(returnUrl), associate)), "/").ToLower();
        }

        public static bool ProcessOAuthAuthentication(string provider, string returnUrl, bool associate)
        {
            var authProvider = CoreServices.Authentication.GetAuthenticationProvider(provider);
            if (authProvider != null)   //todo:  verify it implements this interface or assume we couldn't get here without it
            {
                var result = ((Providers.IOAuthAuthenticationProvider)authProvider).VerifyAuthentication(GetOAuthLoginCallbackUrl(provider, returnUrl, associate));
                if (result.Success)
                {
                    if (associate)
                    {
                        if (IsAuthenticated)
                        {
                            CoreServices.Authentication.AssociateAuthenticationToken(CoreServices.Account.CurrentUser, result.Provider, result.ProviderUserId);
                            applyAuthenticationResultToUser(result, CoreServices.Account.CurrentUser);
                        }
                        else
                            throw new Exception("Cannot associate Authentication without logged in user");
                    }
                    else
                        processAuthenticationResult(result, true);
                    return true;
                }
                else
                    throw new Exception("External Authentication Failed: " + result.Errors.ToJson());//return RedirectToAction("OAuthLoginFailure");
            }
            else
                throw new Exception("Authentication Provider not found: " + provider);

        }

        public static LoginResult Login(string userName, string password, bool persistant, string provider)
        {
            var ret = new LoginResult();
            var authResult = Authenticate(userName, password, provider);
            if (authResult.Success)
            {
                var user = processAuthenticationResult(authResult, persistant);
                ret.UserId = user.Id;
                ret.MustVerify = Account.AccountVerificationMode == "Passive" && !user.IsEmailVerified;  //Enforced will take care of it for us
            }
            else if (SupportsReset)
            {
                var resetResult = AuthenticationResetProvider.Authenticate(userName, password);
                if (resetResult.Authenticated)
                {
                    var user = Account.GetUser(userName);
                    issueAuthenticationTicket(user, true);
                    ret.UserId = user.Id;
                    ret.MustChangePassword = true;
                    ret.MustVerify = Account.AccountVerificationMode == "Passive" && !user.IsEmailVerified;  //Enforced will take care of it for us
                }
            }
            return ret;
        }

        public static AuthenticationResult Authenticate(string userName, string password, string provider)
        {
            var authProvider = CoreServices.Authentication.GetAuthenticationProvider(provider);
            if (authProvider != null)   //todo:  verify it implements this interface or assume we couldn't get here without it
                return ((Providers.IStandardAuthenticationProvider)authProvider).Login(userName, password);
            else
                throw new Exception("Authentication Provider not found: " + provider);
        }

        public static bool SupportsPersistantCreate
        {
            get
            {
                //todo:  perform check on Persistence Provider as IAuthenticationProvider to see if AllowCreation is turned on?
                return CoreServices.Authentication.PersistenceProvider != null && !string.IsNullOrEmpty(CoreServices.Portal.GetPortalAttribute("Account", "CreateAccountUrl", ""));
            }
        }

        public static bool SupportsReset { get { return AuthenticationResetProvider != null; } }

        public static Services.AuthenticationResetResult IssueAuthenticationResetTicket(string userName, string url)
        {
            var errors = new List<string>();
            if (SupportsReset)
            {
                var user = Account.GetUser(userName);
                if (user != null)
                    return AuthenticationResetProvider.IssueResetTicket(user.Id, url);
                else
                    errors.Add(Localization.GetLocalization(LocalizationType.Exception, "UserNotFound.Error", "User not found.", "Core"));
            }
            else
                errors.Add(Localization.GetLocalization(LocalizationType.Exception, "ResetNotEnabled.Error", "Reset not enabled.", "Core"));
            return new AuthenticationResetResult() { Success = false, Errors = errors };
        }

        public static void RegisterAuthenticationResetProviders()
        {
            ObjectFactory.Configure(x =>
                x.Scan(scan =>
                {
                    scan.AssembliesFromApplicationBaseDirectory();
                    scan.AddAllTypesOf<IAuthenticationResetProvider>();
                }));
            _authenticationResetProviders = ObjectFactory.GetAllInstances<IAuthenticationResetProvider>().ToList();
            foreach (var provider in _authenticationResetProviders)
                provider.Register();

            var providers = new List<string>() { "" };
            providers.AddRange(_authenticationResetProviders.Select(b => b.Name));

            var updates = CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = "AuthenticationResetProvider", Values = providers, DefaultValue = "", Required = false, LabelKey = "AuthenticationProvider.Text", LabelText = "Authentication Reset Provider" });

            if (updates > 0)
                CoreServices.Repository.SaveChanges();

            if (AuthenticationResetProvider != null)
                AuthenticationResetProvider.InitializePersistence(Portal.GetAppSetting("AuthenticationResetConnection", ""));

        }

        public static List<IAuthenticationResetProvider> GetAuthenticationResetProviders()
        {
            return _authenticationResetProviders;
        }

        public static IAuthenticationResetProvider AuthenticationResetProvider
        {
            get
            {
                return GetAuthenticationResetProviders().Where(p => p.Name == Services.Portal.GetPortalSetting("Authentication", "AuthenticationResetProvider", "")).FirstOrDefault();
            }
        }

        //Note:  this is NOT the logic used in account association to a new authentication provider
        private static Models.User processAuthenticationResult(AuthenticationResult authResult, bool persistant)
        {
            var user = GetUserByAuthenticationToken(authResult.Provider, authResult.ProviderUserId);

            //if authenticated but not existant, we want to create one
            if (user == null)
            {
                if (GetAuthenticationProvider(authResult.Provider).AllowCreation)
                {
                    if (Account.GetUser(authResult.UserName) != null)   //do NOT allow a user to associate himself with an existing user if authenticates against system with same name... would be security hole as user could take over account that is not his
                        throw new Exception("Cannot create new user from authentication.  User already exists in system with same username");

                    user = new Models.User()
                    {
                        Name = authResult.UserName,
                        PortalId = Portal.CurrentPortalId
                    };
                    user.Id = Account.SaveUser(user); //must save before we associate
                    Authentication.AssociateAuthenticationToken(user, authResult.Provider, authResult.ProviderUserId);
                }
                else
                    throw new Exception("User does not exist in system and the authentication provider you are using is not configured to allow new account creation");
            }

            if (user != null)
            {
                applyAuthenticationResultToUser(authResult, user);
                issueAuthenticationTicket(user, persistant);
            }

            return user;
        }

        private static void issueAuthenticationTicket(Models.User user, bool persistant)
        {
            Authentication.IssueAuthenticationTicket(user.Id, user.RoleIds, 30, persistant); //todo: make expire days configurable?
        }

        private static void applyAuthenticationResultToUser(AuthenticationResult authResult, Models.User user)
        {
            var changes = 0;
            //todo: optimize?  dictionary or something...
            if (authResult.Claims != null)
            {
                foreach (var claim in authResult.Claims)
                {
                    if (setClaimValue(user, claim.Type, claim.Issuer, claim.Value))
                        changes++;
                    //if (user.GetClaim(claim.Type, claim.Issuer) == null)
                    //{
                    //    user.Claims.Add(claim);
                    //    changes++;
                    //}
                }
            }

            if (setClaimValue(user, _authenticationAccountNameClaimType, authResult.Provider, authResult.UserName))
                changes++;

            //user.Claims.AddRange(authResult.Claims);
            if (authResult.ExtraData != null)
            {
                foreach (var key in authResult.ExtraData.Keys)
                {
                    //if provider is returning an email and we don't have email yet, use it
                    if (key.Equals("email", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(user.Email))
                        {
                            user.Email = authResult.ExtraData[key];
                            changes++;
                        }
                    }
                    else if (key.Equals("locale", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(user.Locale))
                        {
                            user.Locale = authResult.ExtraData[key];
                            changes++;
                        }
                    }
                    else if (key.Equals("displayname", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(user.DisplayName))
                        {
                            user.DisplayName = authResult.ExtraData[key];
                            changes++;
                        }
                    }
                    else
                    {
                        if (!user.Attributes.ContainsKey(key) || !user.Attributes[key].ToString().Equals(authResult.ExtraData[key], StringComparison.InvariantCultureIgnoreCase))
                        {
                            //if it is an element user can edit, then don't overwrite
                            if (!user.Attributes.ContainsKey(key) || !CoreServices.Account.CustomUserElements.Exists(e => e.Name == key && e.UserCanEdit))
                            {
                                user.Attributes[key] = authResult.ExtraData[key];
                                changes++;
                            }
                        }
                    }
                }
            }

            //allow provider to create roles and associate them
            foreach (var roleName in authResult.Roles)
            {
                var role = Account.GetRole(roleName);
                if (role == null)
                {
                    role = new CoreModels.Role() { Name = roleName, Description = "Auto created from " + authResult.Provider, Issuer = authResult.Provider };
                    role.Id = Account.SaveRole(role);
                }
                if (!user.RoleIds.Contains(role.Id))
                {
                    user.RoleIds.Add(role.Id);
                    changes++;
                }
            }

            if (changes > 0)
                Account.SaveUser(user); //for now we will persist any information coming back from the authentication provider...  may change mind
        }


    }
}
