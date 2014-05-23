using CodeEndeavors.Extensions;
using Newtonsoft.Json.Linq;
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
    public class VidereAuthenticationProvider : IStandardAuthenticationProvider, IAuthenticationPersistance
    {
        public string Name
        {
            get { return "Videre"; }
        }

        public string LoginButtonText
        {
            get { return CoreServices.Localization.GetPortalText("Login.Text", "Login"); }
        }

        //public bool Enabled
        //{
        //    get
        //    {
        //        //STACK OVERFLOW
        //        //if (CoreServices.Authentication.GetAuthenticationProviders().Where(p => p.Enabled).Count() == 0)
        //        //    return true;
        //        return CoreServices.Portal.GetPortalAttribute("Authentication", Name, true);
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
            var updates = CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = Name + "-Options", DefaultValue = new JArray() {"Allow Association","Allow Login","Allow Creation"}, LabelKey = Name + "Options.Text", LabelText = Name + " Options", Multiple = true, ControlType = "bootstrap-select", Values = new List<string>() { "Allow Association", "Allow Creation", "Allow Login" } });

            if (updates > 0)
                CoreServices.Repository.SaveChanges();
        }

        public void InitializePersistance(string connection)
        {
            var updates = migrateToSeparateDataStore();
            if (updates > 0)
                CoreServices.Repository.SaveChanges();
        }

        public Services.AuthenticationResult Login(string userName, string password)
        {
            var user = CoreServices.Repository.Current.GetResourceData<Models.UserAuthentication>("UserAuthentication", u => u.Data.Name == userName, null);

            if (user != null && user.PasswordHash != GeneratePasswordHash(password, user.PasswordSalt))
                user = null;

            return new CoreServices.AuthenticationResult()
            {
                Success = user != null,
                Provider = Name,
                //ProviderUserId = user != null ? user.UserId : null,
                ProviderUserId = user != null ? user.Id : null, //NOT LOGIN ID - TOKEN!
                UserName = userName
            };
        }

        private string GeneratePasswordHash(string password, string salt)
        {
            return FormsAuthentication.HashPasswordForStoringInConfigFile(string.Concat(password, salt), "md5");
        }

        private string GenerateSalt()
        {
            var random = new Byte[64];
            var rng = new RNGCryptoServiceProvider();
            rng.GetNonZeroBytes(random);
            return Convert.ToBase64String(random);
        }

        public CoreServices.AuthenticationResult SaveAuthentication(string userId, string userName, string password)
        {
            //var result = CoreServices.Repository.Current.GetResourceById<Models.UserAuthentication>(id);
            var userAuth = CoreServices.Repository.Current.GetResourceData<Models.UserAuthentication>("UserAuthentication", a => a.Data.UserId == userId, new Models.UserAuthentication() { UserId = userId });
            userAuth.Name = userName;
            userAuth.PasswordSalt = GenerateSalt();
            userAuth.PasswordHash = GeneratePasswordHash(password, userAuth.PasswordSalt);
            CoreServices.Repository.Current.StoreResource("UserAuthentication", null, userAuth, userId);

            return new CoreServices.AuthenticationResult()
            {
                Success = true,
                UserName = userName,
                Provider = Name,
                ProviderUserId = userAuth.Id
            };
        }

        //this is needed while we still have our migration code in place
        private static AccountProviders.IAccountService _accountService;
        private static AccountProviders.IAccountService AccountService
        {
            get
            {
                if (_accountService == null)
                {
                    _accountService = ConfigurationManager.AppSettings.GetSetting("AccountServicesProvider", "Videre.Core.AccountProviders.VidereAccount, Videre.Core").GetInstance<AccountProviders.IAccountService>();
                    _accountService.Initialize(ConfigurationManager.AppSettings.GetSetting("AccountServicesConnection", ""));
                }
                return _accountService;
            }
        }

        //migrate user password info into own datastore
        private int migrateToSeparateDataStore()
        {
            var count = 0;
            if (AccountService is Videre.Core.AccountProviders.VidereAccount)   //only do this conversion if using VidereAccount service...  for now... 
            {
                var users = AccountService.Get(CoreServices.Portal.CurrentPortalId).Where(u => u.PasswordHash != null).ToList();
                foreach (var user in users)
                {
                    var auth = new Models.UserAuthentication() { UserId = user.Id, Name = user.Name, PasswordHash = user.PasswordHash, PasswordSalt = user.PasswordSalt };
                    CoreServices.Repository.Current.StoreResource("UserAuthentication", null, auth, user.Id);

                    //reset
                    user.PasswordSalt = null;
                    user.PasswordHash = null;
                    AccountService.Save(user, user.Id);

                    count++;
                }
            }

            return count;
        }
    }
}
