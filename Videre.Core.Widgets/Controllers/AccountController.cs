using CodeEndeavors.Extensions;
using System.Web.Mvc;
using Videre.Core.ActionResults;
using System.Collections.Generic;
using Videre.Core.Services;
using System.Web.Security;
using System;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Widgets.Controllers
{
    public class AccountController : Controller
    {
        public JsonResult<Dictionary<string, object>> LogIn(string userName, string password, bool rememberMe, string provider)
        {
            return API.Execute<Dictionary<string, object>>(r =>
            {
                var user = CoreServices.Authentication.Login(userName, password, rememberMe, provider);
                if (user != null)
                    r.Data["user"] = user;
                else
                    r.AddError("The user name or password provided is incorrect.");
            });
        }

        public ActionResult OAuthLogin(string provider, string returnUrl)
        {
            return new OAuthAuthenticationResult(provider, CoreServices.Authentication.GetOAuthLoginCallbackUrl(provider, returnUrl, false));
        }

        public ActionResult AssociateOAuthLogin(string provider, string returnUrl)
        {
            return new OAuthAuthenticationResult(provider, CoreServices.Authentication.GetOAuthLoginCallbackUrl(provider, returnUrl, true));
        }

        public JsonResult<dynamic> AssociateExternalLogin(string userName, string password, string provider)
        {
            return API.Execute<dynamic>(r =>
            {
                var associated = CoreServices.Authentication.AssociateExternalLogin(Account.CurrentUser.Id, userName, password, provider);
                r.Data = new
                {
                    associated = associated,
                    profile = CoreServices.Account.GetUserProfile(CoreServices.Account.CurrentUser.Id),
                    userAuthProviders = CoreServices.Authentication.GetUserAuthenticationProviders(CoreServices.Account.GetUserById(CoreServices.Account.CurrentUser.Id))
                };
            });
        }

        public JsonResult<dynamic> DisassociateOAuthLogin(string provider)
        {
            return API.Execute<dynamic>(r =>
            {
                if (!CoreServices.Account.IsAuthenticated)
                    throw new Exception(Localization.GetLocalization(CoreModels.LocalizationType.Exception, "AccessDenied.Error", "Access Denied.", "Core"));   //sneaky!

                CoreServices.Authentication.DisassociateAuthenticationToken(CoreServices.Account.CurrentUser.Id, provider);
                r.Data = new
                {
                    profile = CoreServices.Account.GetUserProfile(CoreServices.Account.CurrentUser.Id),
                    userAuthProviders = CoreServices.Authentication.GetUserAuthenticationProviders(CoreServices.Account.GetUserById(CoreServices.Account.CurrentUser.Id))
                };
            });
        }
        
        public ActionResult OAuthLoginCallback(string provider, string returnUrl, bool associate)
        {
            if (CoreServices.Authentication.ProcessOAuthAuthentication(provider, returnUrl, associate))
            {
                //todo: FIX THIS!
                //if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            }
            throw new Exception("Error in External Login Callback");
        }

        public ActionResult LogOff()
        {
            CoreServices.Authentication.RevokeAuthenticationTicket();
            return RedirectToRoute("Route");
        }

        public JsonResult<List<CoreModels.User>> GetUsers()
        {
            return API.Execute<List<CoreModels.User>>(r =>
            {
                r.Data = CoreServices.Account.GetUsers();
            });
        }

        public JsonResult<bool> SaveUser(CoreModels.User user)
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("Account", "Administration");
                r.Data = !string.IsNullOrEmpty(CoreServices.Account.SaveUser(user));
            });
        }

        public JsonResult<bool> DeleteUser(string id)
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("Account", "Administration");
                r.Data = CoreServices.Account.DeleteUser(id);
            });
        }

        public JsonResult<List<CoreModels.Role>> GetRoles()
        {
            return API.Execute<List<CoreModels.Role>>(r =>
            {
                r.Data = CoreServices.Account.GetRoles(); 
            });
        }

        public JsonResult<bool> SaveRole(CoreModels.Role role)
        {
            return API.Execute<bool>(r => {
                CoreServices.Security.VerifyActivityAuthorized("Account", "Administration");
                r.Data = !string.IsNullOrEmpty(CoreServices.Account.SaveRole(role));
            });
        }

        public JsonResult<bool> DeleteRole(string id)
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("Account", "Administration");
                r.Data = CoreServices.Account.DeleteRole(id);
            });
        }

        public JsonResult<CoreModels.UserProfile> GetUserProfile()
        {
            return API.Execute<CoreModels.UserProfile>(r =>
            {
                r.Data = CoreServices.Account.GetUserProfile();
            });
        }

        public JsonResult<bool> SaveUserProfile(CoreModels.UserProfile user)
        {
            return API.Execute<bool>(r =>
            {
                if (user.Id != Account.CurrentIdentityName)
                    throw new Exception(Localization.GetLocalization(CoreModels.LocalizationType.Exception, "AccessDenied.Error", "Access Denied.", "Core"));   //sneaky!

                r.Data = CoreServices.Account.SaveUserProfile(user);
            });
        }

    }
}
