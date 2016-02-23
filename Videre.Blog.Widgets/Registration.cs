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
                new CoreModels.WidgetManifest() { Path = "Blog", Name = "Blog", Title = "Blog", EditorPath = "Widgets/Blog/BlogEditor", EditorType = "videre.widgets.editor.blog", ContentProvider = "Videre.Blog.Widgets.ContentProviders.BlogContentProvider, Videre.Blog.Widgets", Category = "Blog", AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition()
                    {
                        Name = "CommentProvider",
                        Values = new List<string>() { "None", "Videre", "Disquss" },
                        DefaultValue = "None",
                        Required = true,
                        LabelKey = "CommentProvider.Text",
                        LabelText = "Comment Provider"
                    },
                    new AttributeDefinition()
                    {
                        Name = "DisqussShortName",
                        DefaultValue = "",
                        Required = true,
                        LabelKey = "DisqussShortName.Text",
                        LabelText = "Disquss Short Name",
                        Dependencies = new List<AttributeDependency>() { new AttributeDependency() { DependencyName = "CommentProvider", Values = new List<string>() { "Disquss" }}}
                    },
                    new AttributeDefinition()
                    {
                        Name = "ShowBlogTitle",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "Yes",
                        Required = true,
                        LabelKey = "ShowBlogTitle.Text",
                        LabelText = "Show Blog Title"
                    }
                }
                },
                new CoreModels.WidgetManifest() { Path = "Blog", Name = "BlogTags", Title = "Blog Tags", EditorType = "videre.widgets.editor.blog",  ContentProvider = "Videre.Blog.Widgets.ContentProviders.BlogContentProvider, Videre.Blog.Widgets", Category = "Blog" },
                new CoreModels.WidgetManifest() { Path = "Blog", Name = "BlogArchive", Title = "Blog Archive", EditorType = "videre.widgets.editor.blog",  ContentProvider = "Videre.Blog.Widgets.ContentProviders.BlogContentProvider, Videre.Blog.Widgets", Category = "Blog" },
                new CoreModels.WidgetManifest() { Path = "Blog", Name = "LatestBlogEntry", Title = "Latest Blog Entry", EditorType = "videre.widgets.editor.blog",  ContentProvider = "Videre.Blog.Widgets.ContentProviders.BlogContentProvider, Videre.Blog.Widgets", Category = "Blog", AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition()
                    {
                        Name = "ShowBlogTitle",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "Yes",
                        Required = true,
                        LabelKey = "ShowBlogTitle.Text",
                        LabelText = "Show Blog Title"
                    }
                }}
            });

            updates += CoreServices.Update.Register(new CoreModels.SearchProvider()
            {
                Name = "Blog Search Provider",
                ProviderType = "Videre.Blog.Widgets.Services.BlogSearchProvider, Videre.Blog.Widgets",
                DocumentTypes = new List<string>() { "Blog" }
            });

            return updates;
        }

        public int RegisterPortal(string portalId)
        {
            var updates = 0;
            updates += CoreServices.Update.Register(new List<CoreModels.SecureActivity>()
            {
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Blog", Name = "Administration", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} }
            });
            return updates;        
        }
    }
}