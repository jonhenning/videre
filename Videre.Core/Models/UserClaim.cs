using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.Models
{
    public class UserClaim
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public string Issuer { get; set; }
    }
}
