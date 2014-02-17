using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Providers
{
    public class VidereAuthenticationProvider : IStandardAuthenticationProvider
    {
        public string Name
        {
            get { return "Videre"; }
        }

        public string LoginButtonText
        {
            get { return CoreServices.Localization.GetPortalText("Login.Text", "Login "); }
        }

        public bool Enabled
        {
            get 
            {
                //STACK OVERFLOW
                //if (CoreServices.Authentication.GetAuthenticationProviders().Where(p => p.Enabled).Count() == 0)
                //    return true;
                if (CoreServices.Portal.CurrentPortal == null)
                    return true;
                return CoreServices.Portal.CurrentPortal.GetAttribute("Authentication Providers", "Videre", true); 
            }
        }

        public void Register()
        {
            var updates = CoreServices.Update.Register("Authentication Providers", new CoreModels.AttributeDefinition() { Name = "Videre", DefaultValue = "true", LabelKey = "VidereAuthentication.Text", LabelText = "Videre Authentication", DataType = "boolean", InputType = "checkbox", ControlType = "checkbox" });
            if (updates > 0)
                CoreServices.Repository.SaveChanges();
        }

        public Services.AuthenticationResult VerifyAuthentication(string returnUrl)
        {
            throw new NotImplementedException();
        }

        public Services.AuthenticationResult Login(string userName, string password)
        {
            var users = CoreServices.Repository.Current.GetResources<Models.User>("User", m => m.Data.PortalId == CoreServices.Portal.CurrentPortalId, false).Select(f => f.Data).ToList();
            var user = users.Where(u => u.PasswordHash == GeneratePasswordHash(password, u.PasswordSalt)).FirstOrDefault();
            return new CoreServices.AuthenticationResult()
            {
                 Success = user != null,
                 Provider = Name,
                 ProviderUserId = user != null ? user.Id : null,
                 UserName = userName 
            };
        }

        public string GeneratePasswordHash(string password, string salt)
        {
            return FormsAuthentication.HashPasswordForStoringInConfigFile(string.Concat(password, salt), "md5");
        }

        public string GenerateSalt()
        {
            var random = new Byte[64];
            var rng = new RNGCryptoServiceProvider();
            rng.GetNonZeroBytes(random);
            return Convert.ToBase64String(random);
        }

    }
}
