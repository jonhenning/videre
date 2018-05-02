using CodeEndeavors.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Videre.Core.Models;
using Videre.Core.Services;

namespace Videre.Core.Providers
{
    public class VidereVerifyAccountProvider : IVerifyAccountProvider
    {
        public string Name
        {
            get
            {
                return "Videre";
            }
        }

        public void Initialize(string connection)
        {

        }

        public bool IsAccountVerified(IAuthorizationUser user)
        {
            var userVerified = user.Claims.Where(c => c.Type.Equals("Account Verified On", System.StringComparison.InvariantCultureIgnoreCase) && c.Issuer.Equals("Videre Account Verification", System.StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() != null;
            var browserVerified = userBrowserIsVerified(user.Id);
            return userVerified && browserVerified;
            //return user.IsEmailVerified;
            //return user.GetClaimValue<string>("Account Verified On", "Videre Account Verification", null) != null;
        }

        public string GenerateVerificationCode()
        {
            return System.Web.Security.Membership.GeneratePassword(8, 2);
        }

        public bool SendVerification(User user, string code)
        {
            var subject = Services.Localization.GetPortalText("PortalEmailAccountVerificationSubject.Text", "Account Verification Code");
            var body = Services.Localization.GetPortalText("PortalEmailAccountVerificationBody.Text", "<p>Please verify your account by logging into <a href=\"$Url\">your account</a> and entering the following code when asked.</p><p><b><a href=\"$Url\">$Code</a></b></p>");
            var tokens = new Dictionary<string, object>()
                {
                    {"Code", code},
                    {"Url", Services.Portal.RequestRootUrl.PathCombine(Account.AccountVerificationUrl) + "?code=" + HttpUtility.UrlEncode(code)}
                };
            if (!string.IsNullOrEmpty(Services.Portal.CurrentPortal.AdministratorEmail))
                Services.Mail.Send(Services.Portal.CurrentPortal.AdministratorEmail, user.Email, "AccountVerification", subject, body, tokens, true);
            else
                throw new Exception(Services.Localization.GetExceptionText("AdministratorEmailNotSet.Text", "Administrator Email not set.  Please contact the portal administrator."));
            return true;
        }

        public bool IssueAccountVerificationCode(User user, string code)
        {
            var claim = user.GetClaim("Account Verification Code", "Videre Account Verification");
            if (claim == null)
            {
                claim = new UserClaim() { Type = "Account Verification Code", Issuer = "Videre Account Verification", Value = code };
                user.Claims.Add(claim);
                return true;
            }
            return false;
        }

        public bool ForceVerifyAccount(Models.User user)
        {
            if (!Account.IsAccountVerified(user)) //if (!user.IsEmailVerified)
            {
                user.Claims.Add(new UserClaim() { Type = "Account Verified On", Issuer = "Videre Account Verification", Value = DateTime.UtcNow.ToJson() });
                return true;
            }
            return false;
        }

        public bool VerifyAccount(Models.User user, string verificationCode, bool trust)
        {
            if (user.GetClaimValue("Account Verification Code", "Videre Account Verification", "") == verificationCode)
            {
                user.Claims.Add(new UserClaim() { Type = "Account Verified On", Issuer = "Videre Account Verification", Value = DateTime.UtcNow.ToJson() });

                //if (UserRequiresTwoPhaseVerification(user.Id) && !UserBrowserIsVerified())
                //    MarkUserBrowserVerified(trust);

                var claim = user.GetClaim("Account Verification Code", "Videre Account Verification");
                if (claim != null)
                    user.Claims.Remove(claim);

                if (!userBrowserIsVerified())
                    markUserBrowserVerified(trust);
                return true;
            }
            return false;
        }

        public bool RemoveAccountVerification(Models.User user)
        {
            var claims = user.Claims.Where(c => c.Issuer == "Videre Account Verification").ToList();
            foreach (var claim in claims)
                user.Claims.Remove(claim);

            if (!userBrowserIsVerified())
                markUserBrowserVerified(false, user.Id);    //expire cookie

            return true;
        }


        private bool userBrowserIsVerified(string userId = null)
        {
            var cookie = HttpContext.Current.Request.Cookies["browserverification-" + userId];
            return cookie != null && cookie.Value == userId;    //todo:  value should be something simple like userId or move complicated like a hash of the userAgent???
        }

        private void markUserBrowserVerified(bool trust, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Authentication.AuthenticatedUserId : userId;
            var cookie = new HttpCookie("browserverification-" + userId, userId);
            cookie.HttpOnly = true;
            if (trust)
                cookie.Expires = DateTime.Now.AddDays(ConfigurationManager.AppSettings.GetSetting("UserBrowserVerificationDays", 30));
            else
                cookie.Expires = DateTime.MinValue; //session cookie

            HttpContext.Current.Response.AppendCookie(cookie);
        }

    }
}
