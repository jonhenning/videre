using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web.Mvc;
using System.Xml;

namespace Videre.Core.ActionResults
{
    public class AtomResult : ActionResult
    {
        SyndicationFeed feed;

        public AtomResult() { }
        public AtomResult(SyndicationFeed feed)
        {
            this.feed = feed;
        }
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "application/atom+xml";
            var formatter = new Atom10FeedFormatter(this.feed);
            using (var writer = XmlWriter.Create(context.HttpContext.Response.Output))
            {
                formatter.WriteTo(writer);
            }
        }
    }
}
