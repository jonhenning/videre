using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.Models
{
    public class Comment
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Text { get; set; }
        //public bool Official { get; set; }
        public string ImageUrl
        {
            get
            {
                return string.Format("http://www.gravatar.com/avatar/{0}?s=32", System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(Email, "md5").ToLower());
            }
        }
        public DateTime? ApprovedDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
