using Microsoft.Web.WebPages.OAuth;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Videre.Core.Providers;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.OAuth
{
    public class GoogleOAuthProvider : IOAuthAuthenticationProvider
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

        //public bool Enabled
        //{
        //    get 
        //    {
        //        return CoreServices.Portal.CurrentPortal.GetAttribute("Authentication", "Google", false);
        //    }
        //}

        private List<string> Options
        {
            get
            {
                var options = CoreServices.Portal.GetPortalAttribute<JArray>("Authentication", Name + "-Options", new JArray()) ?? new JArray();
                return options.Select(j => j.ToString()).ToList();
            }
        }

        public bool AllowAssociation { get { return Options.Contains("Allow Association"); } }
        public bool AllowLogin { get { return Options.Contains("Allow Login"); } }
        public bool AllowCreation { get { return Options.Contains("Allow Creation"); } }

        public void Register()
        {
            var updates = CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = Name + "-Options", DefaultValue = new JArray() { }, LabelKey = Name + "Options.Text", LabelText = Name + " Options", Multiple = true, ControlType = "bootstrap-select", Values = new List<string>() { "Allow Association", "Allow Creation", "Allow Login" } });

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