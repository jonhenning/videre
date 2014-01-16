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

        [SerializeIgnore(new string[] { "db", "client" })]
        public string DisplaySummary
        {
            get
            {
                var ret = Body;
                if (!string.IsNullOrEmpty(Summary))
                    ret = Summary;
                if (!string.IsNullOrEmpty(Body) && Body.Length > 100)
                    ret = Body.Substring(0, 100);
                return Videre.Core.Services.TokenParser.ReplaceTokensWithContent(ret);
            }
        }

        [SerializeIgnore(new string[] { "client" })]
        public string Body { get; set; }
        
        [SerializeIgnore(new string[] { "db" })]
        public string DisplayBody 
        {
            get
            {
                return Videre.Core.Services.TokenParser.ReplaceTokensWithContent(Body);
            }
            set
            {
                Body = Videre.Core.Services.TokenParser.ReplaceContentWithTokens(value);
            }
        }
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

    }
}