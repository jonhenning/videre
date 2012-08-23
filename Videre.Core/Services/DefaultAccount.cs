using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace Videre.Core.Services
{
    public class DefaultAccount : IAccountService
    {
        public bool ReadOnly { get { return false ; } }

        public void Initialize(string Connection)
        {
        }

        public Models.User Login(string userName, string password)
        {
            var users = Get(null, u => u.Name == userName);
            return users.Where(u => u.PasswordHash == GeneratePasswordHash(password, u.PasswordSalt)).FirstOrDefault();
        }

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
            if (!string.IsNullOrEmpty(user.Password))
            {
                user.PasswordSalt = GenerateSalt();
                user.PasswordHash = GeneratePasswordHash(user.Password, user.PasswordSalt);
                user.Password = null;
            }
            //else if (!string.IsNullOrEmpty(user.Id))    //todo:  hmmm, is this the best way to do this?
            //{
            //    var existing = GetById(user.Id);
            //    user.PasswordHash = existing.PasswordHash;
            //}
            var res = Repository.Current.StoreResource("User", null, user, userId);
            return res.Id;
        }

        public bool Delete(string id, string userId)
        {
            throw new NotImplementedException();
        }

        public Models.User GetById(string id)
        {
            var res = Repository.Current.GetResourceById<Models.User>(id);
            if (res != null)
                return res.Data;
            return null;
        }

        private static string GeneratePasswordHash(string password, string salt)
        {
            return FormsAuthentication.HashPasswordForStoringInConfigFile(string.Concat(password, salt), "md5");
        }

        private static string GenerateSalt()
        {
            var random = new Byte[64];
            var rng = new RNGCryptoServiceProvider();
            rng.GetNonZeroBytes(random);
            return Convert.ToBase64String(random);
        }

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

        public List<Models.CustomDataElement> CustomUserElements
        {
            get { return new List<Models.CustomDataElement>(); }
        }
    }
}
