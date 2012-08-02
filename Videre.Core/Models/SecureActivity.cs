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
            Roles = new List<string>();
        }

        public string Id { get; set; }
        public string PortalId { get; set; }
        public string Area { get; set; }
        public string Name { get; set; }
        public List<string> Roles { get; set; }
        public bool? Authenticated { get; set; }
    }
}
