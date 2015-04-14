using CodeEndeavors.Extensions;
using System;
using System.Web;
using System.Web.Security;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Providers
{
    public class VidereAccountChangePasswordHandler : IRequestHandler
    {
        public string Name
        {
            get { return "Videre Account Change Password Handler"; }
        }

        public int Priority { get { return 2; } }

        public void Execute(string url, Models.PageTemplate template)
        {
            if (CoreServices.Authentication.IsAuthenticated && CoreServices.Authentication.PasswordExpiresDays.HasValue)
            {
                if (CoreServices.Authentication.AuthenticatedUser.MustChangePassword)
                {
                    var loginUrl = CoreServices.Authentication.LoginUrl;
                    var fullUrl = CoreServices.Portal.ResolveUrl("~/" + url);
                    if (!string.IsNullOrEmpty(loginUrl) && fullUrl.IndexOf(loginUrl, StringComparison.InvariantCultureIgnoreCase) == -1 && fullUrl.IndexOf("admin/portal", StringComparison.InvariantCultureIgnoreCase) == -1)  //allow access to portal to change this setting
                        HttpContext.Current.Response.Redirect(loginUrl + "?ForceChangePassword=1&ReturnUrl=" + HttpUtility.UrlEncode(url));
                }
            }

        }

    }
}
