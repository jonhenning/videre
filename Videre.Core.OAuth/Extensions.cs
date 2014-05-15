using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Videre.Core.Providers;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.OAuth
{
    public static class Extensions
    {
        public static CoreServices.AuthenticationResult ToResult(this AuthenticationResult result)
        {
            var ret = new CoreServices.AuthenticationResult()
            {
                Success = result.IsSuccessful,
                ExtraData = result.ExtraData,
                Provider = result.Provider,
                ProviderUserId = result.ProviderUserId,
                UserName = result.UserName,
                SupportsAccountCreation = CoreServices.Portal.CurrentPortal.GetAttribute("Authentication", "GoogleAllowCreation", false)
            };
            if (result.Error != null)
                ret.Errors.Add(result.Error.Message);
            return ret;
        }
    }
}