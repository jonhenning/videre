using CodeEndeavors.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Providers
{
    public class VidereAuthenticationResetProvider : IAuthenticationResetProvider
    {
        public string Name
        {
            get { return "Videre Authentication Reset"; }
        }

        public void Register()
        {
            //var updates = CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = Name + "-ResetTicketHours", DefaultValue = "3", LabelKey = Name + "ResetTicketHours.Text", LabelText = "Allow Account Association - " + Name, DataType = "boolean", InputType = "checkbox", ControlType = "checkbox" });

            //if (updates > 0)
            //    CoreServices.Repository.SaveChanges();
        }

        public void InitializePersistance(string connection)
        {
            var oldTicketDate = DateTime.UtcNow.AddDays(-30);
            var oldTickets = CoreServices.Repository.Current.GetResources<Models.AuthenticationResetTicket>("AuthenticationResetTicket", t => t.Data.IssuedDate < oldTicketDate || t.Data.FulfilledDate.HasValue);
            foreach (var ticket in oldTickets)
                CoreServices.Repository.Current.Delete(ticket);
            if (oldTickets.Count > 0)
                CoreServices.Repository.Current.SaveChanges();
        }

        public Services.AuthenticationResetResult Authenticate(string userName, string password)
        {
            var user = CoreServices.Account.GetUser(userName);
            var ret = new Services.AuthenticationResetResult() { Provider = Name };
            if (user != null)
            {
                ret.Ticket = GetOutstandingTicket(user.Id);
                if (ret.Ticket != null)
                {
                    if (ret.Ticket.PasswordHash == GeneratePasswordHash(password))
                    {
                        ret.Ticket.FulfilledDate = DateTime.UtcNow;
                        CoreServices.Repository.Current.StoreResource("AuthenticationResetTicket", null, ret.Ticket, user.Id);
                        ret.Authenticated = true;
                    }
                }
            }
            else 
                ret.Errors.Add(CoreServices.Localization.GetLocalization(CoreModels.LocalizationType.Exception, "UserNotFound.Error", "User not found.", "Core"));
            
            ret.Success = ret.Errors.Count == 0;
            return ret;
        }
        
        public Services.AuthenticationResetResult IssueResetTicket(string userId, string url)
        {
            var now = DateTime.UtcNow;
            var user = CoreServices.Account.GetUserById(userId);
            var ret = new Services.AuthenticationResetResult() { Provider = Name };
            
            if (user != null)
            {
                ret.Ticket = GetOutstandingTicket(userId);

                if (ret.Ticket != null) //expire old ticket - generate new one - (can't reuse as we can't determine password from hash)
                {
                    ret.Ticket.ExpirationDate = DateTime.MinValue;
                    CoreServices.Repository.Current.StoreResource("AuthenticationResetTicket", null, ret.Ticket, userId);
                }

                var password = GeneratePassword();
                ret.Ticket = new CoreModels.AuthenticationResetTicket()
                {
                    UserId = userId,
                    Url = url,
                    IssuedDate = now,
                    ExpirationDate = now.AddHours(CoreServices.Portal.GetPortalAttribute("Authentication", "ResetTicketHours", 3)),
                    PasswordHash = GeneratePasswordHash(password)
                };
                CoreServices.Repository.Current.StoreResource("AuthenticationResetTicket", null, ret.Ticket, userId);

                SendResetEmail(user, password, ret.Ticket);

            }
            else
                ret.Errors.Add(CoreServices.Localization.GetLocalization(CoreModels.LocalizationType.Exception, "UserNotFound.Error", "User not found.", "Core"));

            ret.Success = ret.Errors.Count == 0;
            return ret;
        }

        private Models.AuthenticationResetTicket GetOutstandingTicket(string userId)
        {
            return CoreServices.Repository.Current.GetResourceData<Models.AuthenticationResetTicket>("AuthenticationResetTicket", u => u.Data.UserId == userId && u.Data.IssuedDate < DateTime.UtcNow && u.Data.ExpirationDate > DateTime.UtcNow && !u.Data.FulfilledDate.HasValue, null);
        }

        private void SendResetEmail(CoreModels.User user, string password, CoreModels.AuthenticationResetTicket ticket)
        {
            var subject = CoreServices.Localization.GetPortalText("PortalEmailPasswordResetSubject.Text", "Reset your password");
            var body = CoreServices.Localization.GetPortalText("PortalEmailPasswordResetBody.Text", "<p>We received a request to reset the password for your $UserName account.  To reset your password navigate to the following link <a href=\"@ResetUrl\">@ResetUrl</a> and use the following temporary password which will expire on $Expiration</p><p><b>$Password</b></p>");
            var tokens = new Dictionary<string, object>()
                {
                    {"UserName", user.Name},
                    {"Expiration", user.GetUserTime(ticket.ExpirationDate)},
                    {"Password", password},
                    {"ResetUrl", ticket.Url}
                };
            if (!string.IsNullOrEmpty(Services.Portal.CurrentPortal.AdministratorEmail))
                CoreServices.Mail.Send(Services.Portal.CurrentPortal.AdministratorEmail, user.Email, "PasswordReset", subject, body, tokens, true);
            else
                throw new Exception(Services.Localization.GetExceptionText("AdministratorEmailNotSet.Text", "Administrator Email not set.  Please contact the portal administrator."));
        }

        private string GeneratePasswordHash(string password)
        {
            return FormsAuthentication.HashPasswordForStoringInConfigFile(password, "md5");
        }

        private string GeneratePassword()
        {
            return System.Web.Security.Membership.GeneratePassword(8, 2);
        }


    }
}
