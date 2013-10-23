using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//todo: change namespace to just Providers
namespace Videre.Core.AccountProviders
{
    public interface IAccountService
    {
        void Initialize(string connection);
        bool ReadOnly { get; }
        Models.User Login(string userName, string password);
        List<Models.User> Get(string portalId);
        List<Models.User> Get(string portalId, Func<Models.User, bool> statement);
        string Save(Models.User user, string userId);
        void Validate(Models.User user);
        bool Delete(string id, string userId);
        Models.User GetById(string id);

        Models.Role GetRoleById(string id);
        List<Models.Role> GetRoles(string portalId);
        string SaveRole(Models.Role role, string userId);
        bool DeleteRole(string id, string userId);

        List<Models.CustomDataElement> CustomUserElements { get; }
    }
}
