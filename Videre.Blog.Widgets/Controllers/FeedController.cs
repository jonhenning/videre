using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using Videre.Core.ActionResults;
using Videre.Core.Services;
using Models = Videre.Blog.Widgets.Models;
using System.ServiceModel.Syndication;
using CodeEndeavors.Extensions;

namespace Videre.Blog.Widgets.Controllers
{
    public class FeedController : Controller
    {
        public ActionResult Rss(string id)
        {
            //todo: cache!
            var blog = Services.Blog.GetByName(id);
            if (blog != null)
            {
                var rootUrl = Request.Url.GetLeftPart(UriPartial.Authority).PathCombine(Portal.ResolveUrl("~/"), "/");
                var url = Portal.GetPageTemplates().Where(t => t.Widgets.Exists(w => w.ContentIds.Contains(blog.Id))).SelectMany(t => t.Urls).Where(u => u.IndexOf("{entry:string}") > -1).FirstOrDefault();
                if (url == null)
                    url = "";//BAD!

                var latestEntry = blog.Entries.Max(e => e.PostDate);
                var lastUpdate = latestEntry.HasValue ? latestEntry.Value : DateTime.UtcNow;
                var feed = new SyndicationFeed(blog.Name, blog.Description, new Uri(Request.Url.AbsoluteUri), blog.Id, lastUpdate);

                feed.Items = blog.Entries.Where(b => b.PostDate.HasValue).Select(
                    e => new SyndicationItem(e.Title, e.Summary, 
                        new Uri(rootUrl.PathCombine(url.Replace("{entry:string}", e.Url), "/")), 
                        e.Id, e.PostDate.Value)
                    ).ToList();
                return new Videre.Core.ActionResults.RssResult(feed);
            }
            return null;
        }

    }
}
