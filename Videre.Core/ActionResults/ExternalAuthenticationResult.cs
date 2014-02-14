using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.ActionResults
{
    public class ExternalAuthenticationResult : ActionResult
    {
        public ExternalAuthenticationResult(string provider, string returnUrl)
        {
            Provider = provider;
            ReturnUrl = returnUrl;
        }

        public string Provider { get; private set; }
        public string ReturnUrl { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var provider = CoreServices.Authentication.GetAuthenticationProvider(Provider);
            if (provider != null)
                provider.RequestAuthentication(Provider, ReturnUrl);
            throw new Exception("Authentication Provider not found: " + Provider);
        }
    }
}
