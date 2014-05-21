using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.Models
{
    public class AuthenticationResetTicket
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PasswordHash { get; set; }
        public string Url { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime? FulfilledDate { get; set; }
    }
}
