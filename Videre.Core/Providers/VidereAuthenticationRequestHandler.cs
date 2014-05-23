using CodeEndeavors.Extensions;
using System;
using System.Web;
using System.Web.Security;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Providers
{
    public class VidereAuthenticationRequestProvider : IRequestHandler
    {
        public string Name
        {
            get { return "Videre Authentication Request Handler"; }
        }

        public int Priority { get { return 1; } }

        public void Execute(string url, Models.PageTemplate template)
        {
            if (!template.IsAuthorized)
                HttpContext.Current.Response.Redirect(FormsAuthentication.LoginUrl);
                //FormsAuthentication.RedirectToLoginPage(); //todo: use web.config for this, or portal setting?
        }

    }
}
