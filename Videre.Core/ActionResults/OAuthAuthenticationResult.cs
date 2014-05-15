using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.ActionResults
{
    public class OAuthAuthenticationResult : ActionResult
    {
        public OAuthAuthenticationResult(string provider, string returnUrl)
        {
            Provider = provider;
            ReturnUrl = returnUrl;
        }

        public string Provider { get; private set; }
        public string ReturnUrl { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var provider = CoreServices.Authentication.GetAuthenticationProvider(Provider);
            if (provider != null)   //todo:  verify it implements this interface or assume we couldn't get here without it
                ((Providers.IOAuthAuthenticationProvider)provider).RequestAuthentication(Provider, ReturnUrl);
            else 
                throw new Exception("Authentication Provider not found: " + Provider);
        }
    }
}
