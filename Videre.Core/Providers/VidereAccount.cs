using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using Videre.Core.Services;

//todo: change namespace to just Providers
namespace Videre.Core.AccountProviders
{
    public class VidereAccount : IClaimsAccountService
    {
        public bool ReadOnly { get { return false ; } }

        public void Initialize(string Connection)
        {
        }

        //public Models.User Login(string userName, string password)
        //{
        //    var users = Get(null, u => u.Name == userName);
        //    return users.Where(u => u.PasswordHash == GeneratePasswordHash(password, u.PasswordSalt)).FirstOrDefault();
        //}

        public List<Models.User> Get(string portalId)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return Repository.Current.GetResources<Models.User>("User", m => m.Data.PortalId == portalId, false).Select(f => f.Data).ToList();
        }

        public List<Models.User> Get(string portalId, Func<Models.User, bool> statement)
        {
            return Get(portalId).Where(statement).ToList();
        }

        public string Save(Models.User user, string userId)
        {
            var res = Repository.Current.StoreResource("User", null, user, userId);
            return res.Id;
        }

        public bool Delete(string id, string userId)
        {
            var user = Repository.Current.GetResourceById<Models.User>(id);
            if (user != null)
                Repository.Current.Delete(user);
            return user != null;
        }

        public Models.User GetById(string id)
        {
            var res = Repository.Current.GetResourceById<Models.User>(id);
            if (res != null)
                return res.Data;
            return null;
        }

        public Models.User GetByName(string portalId, string name)
        {
            return Get(portalId, u => u.Name == name).FirstOrDefault();
        }

        public Models.User GetByClaim(string portalId, string issuer, string type, string value)
        {
            return Get(portalId, u => u.Claims.Exists(c => c.Issuer == issuer && c.Type == type && c.Value == value)).FirstOrDefault();
        }

        //private static string GeneratePasswordHash(string password, string salt)
        //{
        //    return FormsAuthentication.HashPasswordForStoringInConfigFile(string.Concat(password, salt), "md5");
        //}

        //private static string GenerateSalt()
        //{
        //    var random = new Byte[64];
        //    var rng = new RNGCryptoServiceProvider();
        //    rng.GetNonZeroBytes(random);
        //    return Convert.ToBase64String(random);
        //}

        public Models.Role GetRoleById(string id)
        {
            var res = Repository.Current.GetResourceById<Models.Role>(id);
            if (res != null)
                return res.Data;
            return null;
        }

        public List<Models.Role> GetRoles(string portalId)
        {
            return Repository.Current.GetResources<Models.Role>("Role", m => m.Data.PortalId == portalId, false).Select(f => f.Data).OrderBy(i => i.Name).ToList();
        }

        public string SaveRole(Models.Role role, string userId)
        {
            var res = Repository.Current.StoreResource("Role", null, role, userId);
            return res.Id;
        }

        public bool DeleteRole(string id, string userId)
        {
            var role = Repository.Current.GetResourceById<Models.Role>(id);
            if (role != null)
            {
                if (role.Data.Name == "admin")
                    throw new Exception(Localization.GetExceptionText("CannotChangeCoreData.Error", "{0} is part of the core data structure, it is not allowed to be altered.", "admin"));

                Repository.Current.Delete(role);
            }
            return role != null;
        }

        private List<Models.CustomDataElement> _customUserElements = new List<Models.CustomDataElement>();
        public List<Models.CustomDataElement> CustomUserElements
        {
            get { return _customUserElements; }
        }

        public void Validate(Models.User user)
        {
            Validation.ValidateEmail(user.Email);
            if (string.IsNullOrEmpty(user.Name)) // || (string.IsNullOrEmpty(user.Password) && string.IsNullOrEmpty(user.PasswordHash)))
                throw new Exception(Localization.GetExceptionText("InvalidResource.Error", "{0} is invalid.", "User"));
            if (Account.Exists(user))
                throw new Exception(Localization.GetExceptionText("DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "User"));

            if (!string.IsNullOrEmpty(user.Locale))
            {
                if (!CultureInfo.GetCultures(CultureTypes.AllCultures).Any(culture => culture.Name.Equals(user.Locale, StringComparison.InvariantCultureIgnoreCase)))
                    throw new Exception(Localization.GetExceptionText("InvalidLocaleCode.Error", "Invalid locale code."));
            }

        }

        public List<Models.UserClaim> GetClaims(string portalId)
        {
            return new List<Models.UserClaim>();
        }


    }
}
