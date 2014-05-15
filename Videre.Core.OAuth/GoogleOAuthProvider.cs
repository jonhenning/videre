using Microsoft.Web.WebPages.OAuth;
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

        public bool AllowAssociation { get { return CoreServices.Portal.GetPortalAttribute("Authentication", Name + "-AllowAssociation", true); } }
        public bool AllowLogin { get { return CoreServices.Portal.GetPortalAttribute("Authentication", Name + "-AllowLogin", true); } }
        public bool AllowCreation { get { return CoreServices.Portal.GetPortalAttribute("Authentication", Name + "-AllowCreation", true); } }

        public void Register()
        {
            var updates = CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = Name + "-AllowAssociation", DefaultValue = "true", LabelKey = Name + "AllowAssociation.Text", LabelText = "Allow Account Association - " + Name, DataType = "boolean", InputType = "checkbox", ControlType = "checkbox" });
            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = Name + "-AllowLogin", DefaultValue = "true", LabelKey = Name + "AllowLogin.Text", LabelText = "Allow Authentication - " + Name, DataType = "boolean", InputType = "checkbox", ControlType = "checkbox" });
            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = Name + "-AllowCreation", DefaultValue = "true", LabelKey = Name + "AllowCreation.Text", LabelText = "Allow Account Creation - " + Name, DataType = "boolean", InputType = "checkbox", ControlType = "checkbox" });

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