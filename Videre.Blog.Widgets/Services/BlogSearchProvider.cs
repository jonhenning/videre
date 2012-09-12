using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoreServices = Videre.Core.Services;
using CoreModels = Videre.Core.Models;
using Lucene.Net.Index;

namespace Videre.Blog.Widgets.Services
{
    public class BlogSearchProvider : CoreServices.ISearchProvider
    {
        public List<string> Generate(IndexWriter writer)
        {
            var ret = new List<string>();
            ClearDocuments(writer, "Blog");

            var blogs = Blog.Get();
            var count = 0;
            foreach (var blog in blogs)
            {
                foreach (var entry in blog.Entries)
                {
                    var analyzedValues = new Dictionary<string, string>() { { "title", entry.Title } };
                    analyzedValues["url"] = entry.Url;
                    analyzedValues["portalId"] = blog.PortalId;
                    analyzedValues["blogId"] = blog.Id;
                    analyzedValues["summary"] = entry.Summary;
                    analyzedValues["tags"] = string.Join(" ", entry.Tags);
                    writer.AddDocument(new CoreModels.SearchDocument(entry.Id.ToString(), "Blog", entry.Title, entry.Summary, analyzedValues).Document);
                    count++;
                }
            }
            ret.Add(string.Format(CoreServices.Localization.GetPortalText("GenerateIndex.Text", "Added {0} items to index"), count));
            return ret;
        }

        public Core.Models.SearchResult FormatResult(CoreModels.SearchDocument doc)
        {
            var url = Services.Blog.GetBlogUrl(doc.GetField("blogId", ""), doc.GetField("url", ""));
            return new CoreModels.SearchResult()
            {
                Id = doc.Id,
                Name = doc.Name,
                Summary = doc.Summary.Replace("\r\n", "<br/>"),
                Type = doc.Type,
                Url = url
            };

        }

        public bool IsAuthorized(CoreModels.SearchDocument doc, string userId)
        {
            return true;
        }

        private void ClearDocuments(IndexWriter writer, string type)
        {
            writer.DeleteDocuments(new Term("type", type.ToLower()));
            writer.Commit();
        }

    }

}