using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using System.Web.Mvc;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;
using DotNetOpenAuth.AspNet;

namespace Videre.Core.OAuth.Services
{
    public class OAuth
    {
        public static CoreModels.User GetUserByOAuthToken(string provider, string token, string portalId = null)
        {
            return CoreServices.Account.Get(u => u.Attributes.ContainsKey("OAuthToken_" + provider) && u.Attributes["OAuthToken_" + provider].ToString() == token, portalId);
        }

        public static CoreModels.User AssociateOAuthToken(string userId, string provider, string token)
        {
            var user = CoreServices.Account.GetUserById(userId);
            var existing = GetUserByOAuthToken(provider, token, user.PortalId);
            if (existing == null || existing.Id == user.Id)
            {
                user.Attributes["OAuthToken_" + provider] = token;
                CoreServices.Account.SaveUser(user);
                return user;
            }
            else
                throw new Exception("Token already associated with another user");
        }

        public static CoreModels.User DisassociateOAuthToken(string userId, string provider)
        {
            var user = CoreServices.Account.GetUserById(userId);
            if (user.Attributes.ContainsKey("OAuthToken_" + provider))
            {
                var oAuthCount = user.Attributes.Keys.Where(a => a.StartsWith("OAuthToken_")).Count();
                if (!string.IsNullOrEmpty(user.PasswordHash) || oAuthCount > 1)
                {
                    //OAuth.Disassociate(provider, user.OAuthTokens[provider]);
                    user.Attributes.Remove("OAuthToken_" + provider);
                    CoreServices.Account.SaveUser(user);
                    return user;
                }
                else
                    throw new Exception("User cannot remove last form of credentials");
            }
            else
                throw new Exception("User not associated with OAuth");
        }


        public static List<string> GetRegisteredAuthenticationClients()
        {
            return OAuthWebSecurity.RegisteredClientData.Select(c => c.DisplayName.ToLower()).ToList(); //todo:  DisplayName vs Provider (name)???
        }

        public static void RegisterOAuthClients()
        {
            //todo: add config for all clients!
            if (CoreServices.Portal.CurrentPortal.GetAttribute("OAuth", "Google", false))
                OAuthWebSecurity.RegisterGoogleClient();

            if (!string.IsNullOrEmpty(CoreServices.Portal.CurrentPortal.GetAttribute("OAuth", "MicrosoftClientId", "")) && !string.IsNullOrEmpty(CoreServices.Portal.CurrentPortal.GetAttribute("OAuth", "MicrosoftSecret", "")))
                OAuthWebSecurity.RegisterMicrosoftClient(CoreServices.Portal.CurrentPortal.GetAttribute("OAuth", "MicrosoftClientId", ""), CoreServices.Portal.CurrentPortal.GetAttribute("OAuth", "MicrosoftSecret", ""));
        }

        public static AuthenticationResult VerifyAuthentication(string returnUrl)
        {
            return OAuthWebSecurity.VerifyAuthentication(returnUrl);
        }

    }
}
