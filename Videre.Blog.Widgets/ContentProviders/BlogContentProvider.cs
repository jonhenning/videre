using System.Collections.Generic;
using CodeEndeavors.Extensions;
using Videre.Core.ContentProviders;
using CoreServices = Videre.Core.Services;
namespace Videre.Blog.Widgets.ContentProviders
{
    public class BlogContentProvider : IWidgetContentProvider
    {
        T IWidgetContentProvider.Get<T>(List<string> ids)
        {
            var id = ids.Count > 0 ? ids[0] : "";
            return Services.Blog.GetById(id) as T; //hack:  [0]?   - exception when more than one?
        }

        public string GetJson(List<string> ids)
        {
            var id = ids.Count > 0 ? ids[0] : "";
            return Services.Blog.GetById(id).ToJson(); //todo:  pass in ignoreType? //hack:  [0]? - exception when more than one?
        }

        public Dictionary<string, string> Import(string portalId, string ns, string json, Dictionary<string, string> idMap)
        {
            var ret = new Dictionary<string, string>();
            if (json != null)
            {
                var blog = json.ToObject<Models.Blog>();
                //menu.Name = Namespace;
                //blog.PortalId = portalId;
                if (blog != null)
                    ret[blog.Id] = Services.Blog.Import(blog, portalId);
            }
            return ret;
        }

        public List<string> Save(string ns, string value)
        {
            var ret = new List<string>();
            if (value != null)
            {
                var blog = value.ToObject<Models.Blog>();
                ret.Add(Services.Blog.Save(blog));
            }
            return ret;
        }

        public void Delete(List<string> ids)
        {
            var id = ids.Count > 0 ? ids[0] : "";
            var Carousel = Services.Blog.GetById(id);
            if (Carousel != null)
                Services.Blog.Delete(Carousel.Id);
        }
    }
}