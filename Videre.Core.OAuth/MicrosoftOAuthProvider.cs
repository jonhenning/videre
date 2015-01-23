using System;
using System.Collections.Generic;
using System.Linq;
using Videre.Core.Providers;
using DotNetOpenAuth.GoogleOAuth2;
using Microsoft.Web.WebPages.OAuth;
using Newtonsoft.Json.Linq;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.OAuth
{
    public class MicrosoftOAuthProvider : IOAuthAuthenticationProvider
    {
        public string Name { get { return "Microsoft"; } }
        public bool AllowAssociation { get { return Options.Contains("Allow Association"); } }
        public bool AllowLogin { get { return Options.Contains("Allow Login"); } }
        public bool AllowCreation { get { return Options.Contains("Allow Creation"); } }
        public bool AllowDuplicateAssociation { get { return false; } }

        public string LoginButtonText { get { return CoreServices.Localization.GetPortalText("LoginMicrosoft.Text", "Login with Microsoft"); } }

        private List<string> Options
        {
            get
            {
                var options = CoreServices.Portal.GetPortalAttribute<JArray>("Authentication", Name + "-Options", new JArray()) ?? new JArray();
                return options.Select(j => j.ToString()).ToList();
            }
        }

        private static string _registeredClientId;
        private static string _registeredClientSecret;

        private bool hasConfigurationChanged
        {
            get
            {
                return _registeredClientId != CoreServices.Portal.GetPortalAttribute("Authentication", Name + "-ClientId", "") || 
                _registeredClientSecret != CoreServices.Portal.GetPortalAttribute("Authentication", Name + "-ClientSecret", "");
            }
        }

        private void ensureRegistered()
        {
            if (this.hasConfigurationChanged)
            {
                _registeredClientId = CoreServices.Portal.GetPortalAttribute("Authentication", Name + "-ClientId", "");
                _registeredClientSecret = CoreServices.Portal.GetPortalAttribute("Authentication", Name + "-ClientSecret", "");

                var existing = OAuthWebSecurity.RegisteredClientData.ToList().Where(c => c.DisplayName == "microsoft").FirstOrDefault();
                if (existing != null)
                    throw new Exception("Cannot re-register " + Name + " OAuth provider.  Please restart application for change to take effect");

                if (!string.IsNullOrEmpty(_registeredClientId) && !string.IsNullOrEmpty(_registeredClientSecret))
                    OAuthWebSecurity.RegisterMicrosoftClient(_registeredClientId, _registeredClientSecret);
            }
        }

        public void Register()
        {
            var updates = CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = Name + "-Options", DefaultValue = new JArray() { }, LabelKey = Name + "Options.Text", LabelText = Name + " Options", Multiple = true, ControlType = "bootstrap-select", Values = new List<string>() { "Allow Association", "Allow Creation", "Allow Login" } });

            var tooltipText = "Obtain your ClientId and secret by register your endpoint with the Microsoft account Development Center.  RedirectUrl should be set to ~/core/account/oauthlogincallback.";
            CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = Name + "-ClientId", DefaultValue = "", LabelKey = Name + "ClientId.Text", LabelText = Name + " Client Id", TooltipKey = Name + "OAuthTooltip.Text", TooltipText = tooltipText });
            CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = Name + "-ClientSecret", DefaultValue = "", LabelKey = Name + "ClientSecret.Text", LabelText = Name + " Client Secret" });
        }

        public CoreServices.AuthenticationResult VerifyAuthentication(string returnUrl)
        {
            ensureRegistered();
            return OAuthWebSecurity.VerifyAuthentication(returnUrl).ToResult();
        }

        public void RequestAuthentication(string provider, string returnUrl)
        {
            ensureRegistered();
            OAuthWebSecurity.RequestAuthentication(provider, returnUrl);
        }
    }
}