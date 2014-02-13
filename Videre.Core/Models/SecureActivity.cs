using CodeEndeavors.Extensions.Serialization;
using System.Collections.Generic;

namespace Videre.Core.Models
{
    //public enum AuthenticationStatus    //todo: we want enum defined here?   also why not bool?
    //{
    //    None = 0,
    //    NotAuthenticated = 1,
    //    Authenticated = 2
    //}

    public class SecureActivity
    {
        public SecureActivity()
        {
            //Roles = new List<string>();
        }

        public string Id { get; set; }
        public string PortalId { get; set; }
        public string Area { get; set; }
        public string Name { get; set; }

        private List<string> _roles = new List<string>();
        [System.Obsolete("Use RoleIds")]
        [SerializeIgnore(new string[] { "client" })]
        public List<string> Roles
        {
            get
            {
                return _roles;
            }
            set
            {
                _roles = value;
            }
        }

        private List<string> _roleIds = new List<string>();
        public List<string> RoleIds
        {
            get
            {
                if (_roles != null && _roles.Count > 0)
                {
                    _roleIds.AddRange(_roles);
                    _roles.Clear();
                }
                return _roleIds;
            }
            set
            {
                _roleIds = value;
            }
        }

        public bool? Authenticated { get; set; }
    }
}
