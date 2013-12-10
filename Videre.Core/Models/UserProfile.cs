
using System.Collections.Generic;
namespace Videre.Core.Models
{
    public class UserProfile
    {
        public UserProfile()
        {
            Attributes = new Dictionary<string, object>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Locale { get; set; }
        public string TimeZone { get; set; }
        public Dictionary<string, object> Attributes {get;set;}

        public string Password1 { get; set; }
        public string Password2 { get; set; }
    }
}
