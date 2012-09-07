using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using Videre.Core.ActionResults;
using Videre.Core.Services;
using Models = Videre.Blog.Widgets.Models;

namespace Videre.Blog.Widgets.Controllers
{
    public class AdminController : Controller
    {
        public JsonResult<List<Models.Blog>> Get()
        {
            return API.Execute<List<Models.Blog>>(b =>
            {
                b.Data = Services.Blog.Get();
            });
        }

        public JsonResult<List<Models.Blog>> Delete(string id)
        {
            return API.Execute<List<Models.Blog>>(r =>
            {
                Security.VerifyActivityAuthorized("Blog", "Administration");
                Services.Blog.Delete(id);
                r.Data = Services.Blog.Get();
            });
        }

        public JsonResult<Models.BlogEntry> GetEntry(string blogId, string entryId)
        {
            return API.Execute<Models.BlogEntry>(b =>
            {
                var blog = Services.Blog.GetById(blogId);
                if (blog != null)
                    b.Data = blog.Entries.Where(e => e.Id == entryId).FirstOrDefault();
            });
        }

        public JsonResult<Models.BlogEntry> SaveEntry(string blogId, Models.BlogEntry entry)
        {
            return API.Execute<Models.BlogEntry>(b =>
            {
                Security.VerifyActivityAuthorized("Blog", "Administration");
                b.Data = Services.Blog.SaveEntry(blogId, entry);
            });
        }

        public JsonResult<bool> DeleteEntry(string blogId, string entryId)
        {
            return API.Execute<bool>(b =>
            {
                Security.VerifyActivityAuthorized("Blog", "Administration");
                b.Data = Services.Blog.DeleteEntry(blogId, entryId);
            });
        }

    }
}
