using Microsoft.Web.WebPages.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Videre.Core.AuthenticationProviders;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.OAuth
{
    public class GoogleOAuthProvider : IAuthenticationProvider
    {
        public string Name
        {
            get
            {
                return "Google";
            }
        }

        public string LoginButtonText
        {
            get
            {
                return CoreServices.Localization.GetPortalText("LoginGoogle.Text", "Login with Google");    //todo:  icon?
            }
        }

        public bool Enabled
        {
            get 
            {
                return CoreServices.Portal.CurrentPortal.GetAttribute("Authentication Providers", "Google", false);
            }
        }

        public void Register()
        {
            CoreServices.Update.Register("Authentication Providers", new CoreModels.AttributeDefinition() { Name = "Google", DefaultValue = "false", LabelKey = "Google.Text", LabelText = "Google", DataType = "boolean", InputType = "checkbox", ControlType = "checkbox" });
            OAuthWebSecurity.RegisterGoogleClient();
        }

        public CoreServices.AuthenticationResult VerifyAuthentication(string returnUrl)
        {
            return OAuthWebSecurity.VerifyAuthentication(returnUrl).ToResult();
        }

        public void RequestAuthentication(string provider, string returnUrl)
        {
            OAuthWebSecurity.RequestAuthentication(provider, returnUrl);
        }

    }
}