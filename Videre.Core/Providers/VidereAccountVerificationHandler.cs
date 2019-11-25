using CodeEndeavors.Extensions;
using System;
using System.Web;
using System.Web.Security;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Providers
{
    public class VidereAccountVerficationHandler : IRequestHandler
    {
        public string Name
        {
            get { return "Videre Account Verification Handler"; }
        }

        public int Priority { get { return 2; } }

        public void Execute(string url, Models.PageTemplate template)
        {
            if (CoreServices.Account.TwoPhaseAuthenticationEnabled) //has the portal enabled two phase authentication either by specifying a AccountVerificationMode IN (Enforced, Passive) OR settup up EnableTwoPhaseAuthentication = YES on portal level which means user can request to have two phase enabled
            {
                if (CoreServices.Authentication.IsAuthenticated)    //only do enforcement if authenticated
                {
                    if (!CoreServices.Account.IsAccountVerified(CoreServices.Authentication.NonImpersonatedAuthenticatedUser)) // determine if user is verified
                    {
                        if ((CoreServices.Account.AccountVerificationMode == "Enforced") || (CoreServices.Account.UserRequiresTwoPhaseVerification()))  // either the portal is enforcing it or the user is enforcing it
                        {
                            var verifyUrl = CoreServices.Account.AccountVerificationUrl;
                            var fullUrl = CoreServices.Portal.ResolveUrl("~/" + url);
                            if (!string.IsNullOrEmpty(verifyUrl) && fullUrl.IndexOf(verifyUrl, StringComparison.InvariantCultureIgnoreCase) == -1 && fullUrl.IndexOf("admin/portal", StringComparison.InvariantCultureIgnoreCase) == -1)  //allow access to portal to change this setting
                                HttpContext.Current.Response.Redirect(verifyUrl + "?ReturnUrl=" + HttpUtility.UrlEncode(url));
                        }
                    }
                }
            }
        }

    }
}
