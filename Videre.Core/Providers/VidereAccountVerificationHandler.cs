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
            if (CoreServices.Authentication.IsAuthenticated && CoreServices.Account.AccountVerificationMode == "Enforced")
            {
                if (!CoreServices.Authentication.AuthenticatedUser.IsEmailVerified)
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
