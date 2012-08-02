
using System.Web.Mvc;
using Videre.Core.ActionResults;
using System.Collections.Generic;
using Videre.Core.Services;
using System.Web.Security;
using System;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Widgets.Controllers
{
    public class AccountController : Controller
    {
        public JsonResult<Dictionary<string, object>> LogIn(string UserName, string Password, bool RememberMe)
        {
            return API.Execute<Dictionary<string, object>>(r =>
            {
                var user = CoreServices.Account.Login(UserName, Password, RememberMe);
                if (user != null)
                    r.Data["user"] = user;
                else
                    r.AddError("The user name or password provided is incorrect.");
            });
        }

        public ActionResult LogOff()
        {
            CoreServices.Account.RevokeAuthenticationTicket();
            return RedirectToRoute("Route");
        }

        public JsonResult<List<CoreModels.User>> GetUsers()
        {
            return API.Execute<List<CoreModels.User>>(r =>
            {
                r.Data = CoreServices.Account.GetUsers();
            });
        }

        public JsonResult<bool> SaveUser(CoreModels.User user)
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("Account", "Administration");
                r.Data = !string.IsNullOrEmpty(CoreServices.Account.SaveUser(user));
            });
        }

        public JsonResult<bool> DeleteUser(string id)
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("Account", "Administration");
                r.Data = CoreServices.Account.DeleteUser(id);
            });
        }

        public JsonResult<List<CoreModels.Role>> GetRoles()
        {
            return API.Execute<List<CoreModels.Role>>(r =>
            {
                r.Data = CoreServices.Account.GetRoles(); 
            });
        }

        public JsonResult<bool> SaveRole(CoreModels.Role role)
        {
            return API.Execute<bool>(r => {
                CoreServices.Security.VerifyActivityAuthorized("Account", "Administration");
                r.Data = !string.IsNullOrEmpty(CoreServices.Account.SaveRole(role));
            });
        }

        public JsonResult<bool> DeleteRole(string id)
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("Account", "Administration");
                r.Data = CoreServices.Account.DeleteRole(id);
            });
        }

        public JsonResult<CoreModels.UserProfile> GetUserProfile()
        {
            return API.Execute<CoreModels.UserProfile>(r =>
            {
                r.Data = CoreServices.Account.GetUserProfile();
            });
        }

        public JsonResult<bool> SaveUserProfile(CoreModels.UserProfile user)
        {
            return API.Execute<bool>(r =>
            {
                if (user.Id != Account.CurrentIdentityName)
                    throw new Exception(Localization.GetLocalization(CoreModels.LocalizationType.Exception, "AccessDenied.Error", "Access Denied.", "Core"));   //sneaky!

                r.Data = CoreServices.Account.SaveUserProfile(user);
            });
        }

    }
}
