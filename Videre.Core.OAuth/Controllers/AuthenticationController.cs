using System;
using System.Web.Mvc;
using Videre.Core.ActionResults;
using Videre.Core.Services;
using CodeEndeavors.Extensions;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.OAuth.Controllers
{
    public class AuthenticationController : Controller
    {
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            var resultUrl = CoreServices.Portal.RequestRootUrl.PathCombine(CoreServices.Portal.ResolveUrl(string.Format("~/OAuth/Authentication/ExternalLoginCallback?returnUrl={0}", System.Web.HttpUtility.UrlEncode(returnUrl))), "/").ToLower();
            return new ActionResults.ExternalLoginResult(provider, resultUrl);
        }

        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            var callbackUrl = CoreServices.Portal.RequestRootUrl.PathCombine(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }), "/").ToLower();
            var result = Services.OAuth.VerifyAuthentication(callbackUrl);
            if (result.IsSuccessful)
            {
                //if authenticated, then we are in user profile, and associating 
                if (CoreServices.Account.IsAuthenticated)
                    Services.OAuth.AssociateOAuthToken(CoreServices.Account.CurrentUser.Id, result.Provider, result.ProviderUserId);
                else
                {
                    var user = Services.OAuth.GetUserByOAuthToken(result.Provider, result.ProviderUserId);
                    if (user != null)
                        CoreServices.Account.IssueAuthenticationTicket(user.Id.ToString(), user.Roles, 30, true);
                    else 
                        throw new Exception("Not implemented creating new user yet...");
                }

                //todo: FIX THIS!
                //if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            }
            else
                return RedirectToAction("ExternalLoginFailure");
        }

        public JsonResult<CoreModels.UserProfile> DisassociateExternalLogin(string provider)
        {
            return API.Execute<CoreModels.UserProfile>(r =>
            {
                if (!CoreServices.Account.IsAuthenticated)
                    throw new Exception(Localization.GetLocalization(CoreModels.LocalizationType.Exception, "AccessDenied.Error", "Access Denied.", "Core"));   //sneaky!

                Services.OAuth.DisassociateOAuthToken(CoreServices.Account.CurrentUser.Id, provider);
                r.Data = CoreServices.Account.GetUserProfile(CoreServices.Account.CurrentUser.Id);
            });
        }

    }
}
