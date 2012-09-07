using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Videre.Blog.Widgets.Models
{
    public class BlogEntry
    {
        public BlogEntry()
        {
            Tags = new List<string>();
        }
        public string Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Body { get; set; }
        public DateTime? PostDate { get; set; }
        public List<string> Tags { get; set; }
    }
}