using CodeEndeavors.Extensions.Serialization;
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
        public string TitleImageUrl { get; set; }
        public string Summary { get; set; }
        public string Body { get; set; }
        public DateTime? PostDate { get; set; }
        public List<string> Tags { get; set; }

        [SerializeIgnore(new string[] { "db", "client" })]
        public bool IsPublished
        {
            get
            {
                return PostDate.HasValue && PostDate.Value < DateTime.UtcNow; //todo: utc?
            }
        }

        [SerializeIgnore(new string[] { "db", "client" })]
        public string DisplaySummary
        {
            get
            {
                if (!string.IsNullOrEmpty(Summary))
                    return Summary;
                if (!string.IsNullOrEmpty(Body) && Body.Length > 100)
                    return Body.Substring(0, 100);
                return Body;
            }
        }

    }
}