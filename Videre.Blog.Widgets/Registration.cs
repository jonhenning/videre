using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Videre.Core.Models;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Blog.Widgets
{
    public class Registration : IWidgetRegistration
    {
        public int Register()
        {
            RouteTable.Routes.MapRoute(
                "Blog_route",
                "blogapi/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                null,
                new string[] { "Videre.Blog.Widgets" }
            );

            var updates = Videre.Core.Services.Update.Register(new List<CoreModels.WidgetManifest>()
            {
                new CoreModels.WidgetManifest() { Path = "Blog", Name = "Blog", Title = "Blog", EditorPath = "Widgets/Blog/BlogEditor", EditorType = "videre.widgets.editor.blog", ContentProvider = "Videre.Blog.Widgets.ContentProviders.BlogContentProvider, Videre.Blog.Widgets", Category = "General" },
                new CoreModels.WidgetManifest() { Path = "Blog", Name = "BlogTags", Title = "Blog Tags", EditorType = "videre.widgets.editor.blog",  ContentProvider = "Videre.Blog.Widgets.ContentProviders.BlogContentProvider, Videre.Blog.Widgets", Category = "General" }
            });

            updates += CoreServices.Update.Register(new List<CoreModels.SecureActivity>()
            {
                new CoreModels.SecureActivity() { PortalId = CoreServices.Portal.CurrentPortalId, Area = "Blog", Name = "Administration", Roles = new List<string>() {CoreServices.Update.AdminRoleId} }
            });

            return updates;
        }

        public int RegisterPortal(string portalId)
        {
            var updates = 0;
            return updates;        
        }
    }
}