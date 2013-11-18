using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoreServices = Videre.Core.Services;
using CodeEndeavors.Extensions;

namespace Videre.Blog.Widgets.Services
{
    public class Blog
    {
        public static Models.Blog GetById(string id)
        {
            var res = CoreServices.Repository.Current.GetResourceById<Models.Blog>(id);
            if (res != null)
            {
                DetokenizeEntries(res.Data);
                return res.Data;
            }
            return null;
        }

        public static List<Models.Blog> Get(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? CoreServices.Portal.CurrentPortalId : portalId;
            var blogs = CoreServices.Repository.Current.GetResources<Models.Blog>("Blog", m => m.Data.PortalId == portalId, false).Select(f => f.Data).ToList();
            return blogs;
        }

        public static Models.Blog GetByName(string name, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? CoreServices.Portal.CurrentPortalId : portalId;
            var blog = CoreServices.Repository.Current.GetResourceData<Models.Blog>("Blog", m => m.Data.PortalId == portalId && m.Data.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase), null);
            DetokenizeEntries(blog);
            return blog;
        }

        public static string Import(Models.Blog blog, string portalId = null, string userId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? CoreServices.Portal.CurrentPortalId : portalId;
            userId = string.IsNullOrEmpty(userId) ? CoreServices.Account.AuditId : userId;
            var existing = GetByName(blog.Name, portalId);
            if (existing != null)
                blog.Id = existing.Id;
            else
                blog.Id = null;
            blog.PortalId = portalId;
            return Save(blog, userId);
        }

        public static string Save(Models.Blog blog, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? CoreServices.Account.CurrentIdentityName : userId;
            blog.PortalId = string.IsNullOrEmpty(blog.PortalId) ? CoreServices.Portal.CurrentPortalId : blog.PortalId;
            Validate(blog);
            TokenizeEntries(blog);
            var res = CoreServices.Repository.Current.StoreResource("Blog", null, blog, userId);
            return res.Id;
        }

        public static Models.BlogEntry SaveEntry(string blogId, Models.BlogEntry entry, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? CoreServices.Account.CurrentIdentityName : userId;
            var blog = Services.Blog.GetById(blogId);
            Validate(blog, entry);

            //parse out tags
            //entry.Tags = entry.Tags.SelectMany(t => t.Split(' ')).ToList();

            if (string.IsNullOrEmpty(entry.Id))
            {
                entry.Id = Guid.NewGuid().ToString();   //could use numbers here.. as only needs to be unique among this blog...
                blog.Entries.Add(entry);
            }
            else
            {
                var index = blog.Entries.FindIndex(e => e.Id == entry.Id);
                blog.Entries[index] = entry;
            }
            Save(blog, userId);
            return entry;
        }

        public static bool DeleteEntry(string blogId, string entryId, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? CoreServices.Account.CurrentIdentityName : userId;
            var blog = Services.Blog.GetById(blogId);

            var index = blog.Entries.FindIndex(e => e.Id == entryId);
            if (index > -1)
                blog.Entries.RemoveAt(index);

            Save(blog, userId);
            return index > 0;
        }

        public static void Validate(Models.Blog blog)
        {
            if (string.IsNullOrEmpty(blog.Name))
                throw new Exception(CoreServices.Localization.GetExceptionText("InvalidResource.Error", "{0} is invalid.", "Blog"));
            if (Exists(blog))
                throw new Exception(CoreServices.Localization.GetExceptionText("DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "Blog"));
        }

        public static void Validate(Models.Blog blog, Models.BlogEntry entry)
        {
            if (blog == null || string.IsNullOrEmpty(entry.Url))
                throw new Exception(CoreServices.Localization.GetExceptionText("InvalidResource.Error", "{0} is invalid.", "Blog Entry"));
            else if (blog.Entries.Exists(e => e.Url.Equals(entry.Url, StringComparison.InvariantCultureIgnoreCase) && e.Id != entry.Id))
                throw new Exception(CoreServices.Localization.GetExceptionText("DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "Blog Entry"));
        }

        public static bool Exists(Models.Blog blog)
        {
            var existing = GetByName(blog.Name, blog.PortalId);
            return existing != null && existing.Name == blog.Name && existing.Id != blog.Id;
        }

        public static bool Delete(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? CoreServices.Account.CurrentIdentityName : userId;
            var blog = CoreServices.Repository.Current.GetResourceById<Models.Blog>(id);
            if (blog != null)
                CoreServices.Repository.Current.Delete(blog);
            return blog != null;
        }

        public static string GetBlogUrl(string blogId, string entryUrl)
        {
            //todo: move this search for template by contentid to central location
            var url = CoreServices.Portal.GetPageTemplatesByContentId(blogId).SelectMany(t => t.Urls).Where(u => u.IndexOf("{entry:string}") > -1).FirstOrDefault();
            if (url == null)
                url = "";//BAD!
            return CoreServices.Portal.RequestRootUrl.PathCombine(url.Replace("{entry:string}", entryUrl), "/");
        }

        public static void TokenizeEntries(Models.Blog blog)
        {
            blog.Entries.ForEach(e => TokenizeEntry(e));
        }

        public static void TokenizeEntry(Models.BlogEntry entry)
        {
            entry.Summary = CoreServices.TokenParser.ReplaceContentWithTokens(entry.Summary);
            entry.Body = CoreServices.TokenParser.ReplaceContentWithTokens(entry.Body);
        }

        public static void DetokenizeEntries(Models.Blog blog)
        {
            if (blog != null)
                blog.Entries.ForEach(e => DetokenizeEntry(e));
        }

        public static void DetokenizeEntry(Models.BlogEntry entry)
        {
            entry.Summary = CoreServices.TokenParser.ReplaceTokensWithContent(entry.Summary);
            entry.Body = CoreServices.TokenParser.ReplaceTokensWithContent(entry.Body);
        }

    }

}