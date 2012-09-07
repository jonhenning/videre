using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Videre.Blog.Widgets.Models
{
    public class Blog
    {
        public Blog()
        {
            Entries = new List<BlogEntry>();
        }

        public string Id { get; set; }
        public string PortalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<BlogEntry> Entries { get; set; }
    }

}