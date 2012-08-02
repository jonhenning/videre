using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.Xml;

namespace Videre.Core.ActionResults
{
    public class RssResult : ActionResult
    {
        SyndicationFeed feed;

        public RssResult() { }

        public RssResult(SyndicationFeed feed)
        {
            this.feed = feed;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "application/rss+xml";
            var formatter = new Rss20FeedFormatter(this.feed);
            using (var writer = XmlWriter.Create(context.HttpContext.Response.Output))
            {
                formatter.WriteTo(writer);
            }
        }
    }
}

