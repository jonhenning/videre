using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Videre.Core.Models;
using System.Web.Mvc;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;
using System.Web.Routing;

namespace Videre.Core.Widgets
{
    public class Registration : IWidgetRegistration
    {
        public int Register()
        {
            RouteTable.Routes.MapRoute(
                "Core_File", // Route name
                "Core/f/{*Url}", // URL with parameters
                new { controller = "file", action = "View" },
                null,
                new string[] { "Videre.Core.Widgets" }
            );

            RouteTable.Routes.MapRoute(
                "Core_default",
                "Core/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                null,
                new string[] { "Videre.Core.Widgets"}
            );


            //App init (for now... eventually may add PortalId)
            var updates = CoreServices.Update.Register("Core", new CoreModels.AttributeDefinition()
            {
                Name = "TextEditor",
                Values = new List<string>() { "Core/CKTextEditor", "Core/CLTextEditor", "Core/WYSIHTML5TextEditor" },
                DefaultValue = "Core/CKTextEditor",
                Required = true,
                LabelKey = "TextEditor.Text",
                LabelText = "Text Editor"
            });

            updates += CoreServices.Update.Register("Core", new CoreModels.AttributeDefinition()
            {
                Name = "ThemeAPIUrl",
                DefaultValue = "http://api.bootswatch.com/",
                Required = true,
                LabelKey = "ThemeAPIUrl.Text",
                LabelText = "Theme API Url"
            });

            CoreServices.Update.Register("Core", new CoreModels.AttributeDefinition() { Name = "SearchIndexDir", DefaultValue = "~/App_Data/SearchIndex", Required = true, LabelKey = "SearchIndexDir.Text", LabelText = "Search Index Directory" });
            CoreServices.Update.Register("Core", new CoreModels.AttributeDefinition() { Name = "SearchUrl", DefaultValue = "~/search", Required = true, LabelKey = "SearchUrl.Text", LabelText = "Search Url" });

            //App init
            updates += CoreServices.Update.Register(new List<CoreModels.WidgetManifest>()
            {
                new CoreModels.WidgetManifest() { Path = "Core/Account", Name = "LogOn", Title = "Log On", Category = "Account" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Account", Name = "UserProfile", Title = "User Profile", Category = "Account" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "Localization", Title = "Localization Admin", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "Template", Title = "Template Admin", Category = "Admin" }, 
                //new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "LayoutTemplate", Title = "Layout Template Admin", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "WidgetManifest", Title = "Widget Manifest Admin", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "Portal", Title = "Portal Admin", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "File", Title = "File Admin", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "FileBrowser", Title = "File Browser", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "Role", Title = "Role Admin", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "User", Title = "User Admin", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "SecureActivity", Title = "Secure Activity Admin", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "Widget", Title = "Widget Admin", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "Security", Title = "Security Admin", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "Search", Title = "Search Admin", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core", Name = "Menu", Title = "Menu", EditorPath = "Widgets/Core/MenuEditor", EditorType = "videre.widgets.editor.menu", ContentProvider = "Videre.Core.Widgets.ContentProviders.MenuContentProvider, Videre.Core.Widgets", Category = "Navigation" , AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition()
                    {
                        Name = "AlwaysOnTop",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "Yes",
                        Required = false,
                        LabelKey = "AlwaysOnTop.Text",
                        LabelText = "Always On Top"
                    },
                    new AttributeDefinition()
                    {
                        Name = "InverseColors",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "No",
                        Required = false,
                        LabelKey = "InverseColors.Text",
                        LabelText = "Inverse Colors"
                    },
                    new AttributeDefinition()
                    {
                        Name = "ShowLogo",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "No",
                        Required = false,
                        LabelKey = "ShowLogo.Text",
                        LabelText = "Show Logo"
                    },
                    new AttributeDefinition()
                    {
                        Name = "ShowSearch",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "No",
                        Required = false,
                        LabelKey = "ShowSearch.Text",
                        LabelText = "Show Search"
                    }
                }
                },
                new CoreModels.WidgetManifest() { Path = "Core", Name = "TextHtml", Title = "Text Html", EditorPath = "Widgets/Core/TextHtmlEditor", EditorType = "videre.widgets.editor.texthtml", ContentProvider = "Videre.Core.ContentProviders.LocalizationContentProvider, Videre.Core", Category = "General"
                    //, AttributeDefinitions= new List<CoreModels.WidgetAttributeDefinition>() { new CoreModels.WidgetAttributeDefinition() { Name = "foo", LabelKey = "foo.text", LabelText = "foo", Required = true}} 
                },
                //new CoreModels.WidgetManifest() { Path = "Core", Name = "Carousel", Title = "Carousel", EditorPath = "Widgets/Core/CarouselEditor", EditorType = "videre.widgets.editor.carousel", ContentProvider = "Videre.Core.Widgets.ContentProviders.CarouselContentProvider, Videre.Core.Widgets", Category = "General" },
                //new CoreModels.WidgetManifest() { Path = "Core", Name = "Blog", Title = "Blog", EditorPath = "Widgets/Core/BlogEditor", EditorType = "videre.widgets.editor.blog", ContentProvider = "Videre.Core.Widgets.ContentProviders.BlogContentProvider, Videre.Core.Widgets", Category = "General" },
                //new CoreModels.WidgetManifest() { Path = "Core", Name = "BlogTags", Title = "Blog Tags", EditorPath = "Widgets/Core/BlogEditor", EditorType = "videre.widgets.editor.blog",  ContentProvider = "Videre.Core.Widgets.ContentProviders.BlogContentProvider, Videre.Core.Widgets", Category = "General" },
                new CoreModels.WidgetManifest() { Path = "Core", Name = "SearchResult", Title = "Search Result", Category = "General" }

            });


            return updates;
        }

        public int RegisterPortal(string portalId)
        {
            var updates = 0;

            //portal Init
            updates += CoreServices.Update.Register(new List<CoreModels.Role>()
            {
                new CoreModels.Role() { Name = "admin", PortalId = portalId, Description = "Administrative Priviledges" }
            });

            //CoreServices.Repository.SaveChanges();

            //portal init
            updates += CoreServices.Update.Register(new List<CoreModels.SecureActivity>()
            {
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Portal", Name = "Administration", Roles = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "PageTemplate", Name = "Administration", Roles = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "LayoutTemplate", Name = "Administration", Roles = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Localization", Name = "Administration", Roles = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "File", Name = "Administration", Roles = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "File", Name = "Upload", Roles = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Account", Name = "Administration", Roles = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Content", Name = "Administration", Roles = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Comment", Name = "Administration", Roles = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Search", Name = "Upload", Roles = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} }//,
                //new CoreModels.SecureActivity() { Area = "Blog", Name = "Administration", Roles = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} }
            });

            //Moved to PortalInstall
            //if (!CoreServices.Account.ReadOnly)
            //{
            //    updates += CoreServices.Update.Register(new List<CoreModels.User>()
            //    {
            //        new CoreModels.User() { PortalId = portalId, Name = "admin", Password = "videre", Roles = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} }
            //    });
            //}

            //updates += CoreServices.Update.Register(new CoreModels.PageTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "TwoPane",
            //    Title = "Home",
            //    Widgets = new List<CoreModels.Widget>() { 
            //        new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/TextHtml").Id, PaneName = "Main" },
            //        new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Account/LogOn").Id, PaneName = "Right", Authenticated = false }
            //    },
            //    Urls = new List<string>() { "" }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.PageTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "General",
            //    Title = "LogOn",
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Account/LogOn").Id, PaneName = "Main" } },
            //    Urls = new List<string>() { "Account/LogOn" }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.PageTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "General",
            //    Title = "User Profile",
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Account/UserProfile").Id, PaneName = "Main" } },
            //    Urls = new List<string>() { "Account/UserProfile" }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.PageTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "General",
            //    Title = "Localization Administration",
            //    Roles = new List<string>() { CoreServices.Update.AdminRoleId },
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Admin/Localization").Id, PaneName = "Main" } },
            //    Urls = new List<string>() { "Admin/Localization" }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.PageTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "General",
            //    Title = "Page Template Administration",
            //    Roles = new List<string>() { CoreServices.Update.AdminRoleId },
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Admin/Template").Id, PaneName = "Main" } },
            //    Urls = new List<string>() { "Admin/PageTemplate" }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.PageTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "General",
            //    Title = "Layout Template Administration",
            //    Roles = new List<string>() { CoreServices.Update.AdminRoleId },
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Admin/Template").Id, PaneName = "Main", Attributes = new Dictionary<string, object>() { { "IsLayout", true } } } },
            //    Urls = new List<string>() { "Admin/LayoutTemplate" }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.PageTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "General",
            //    Title = "File Administration",
            //    Roles = new List<string>() { CoreServices.Update.AdminRoleId },
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Admin/File").Id, PaneName = "Main" } },
            //    Urls = new List<string>() { "Admin/File" }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.PageTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "Blank",
            //    Title = "File Browser",
            //    //Roles = new List<string>() { CoreServices.Update.GetAdminRoleId },
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Admin/FileBrowser").Id, PaneName = "Main" } },
            //    Urls = new List<string>() { "Admin/FileBrowser" }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.PageTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "General",
            //    Title = "Security Administration",
            //    Roles = new List<string>() { CoreServices.Update.AdminRoleId },
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Admin/Security").Id, PaneName = "Main" } },
            //    Urls = new List<string>() { "Admin/Security" }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.PageTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "General",
            //    Title = "User Administration",
            //    Roles = new List<string>() { CoreServices.Update.AdminRoleId },
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Admin/User").Id, PaneName = "Main" } },
            //    Urls = new List<string>() { "Admin/User" }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.PageTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "General",
            //    Title = "Widget Administration",
            //    Roles = new List<string>() { CoreServices.Update.AdminRoleId },
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Admin/Widget").Id, PaneName = "Main" } },
            //    Urls = new List<string>() { "Admin/Widget" }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.PageTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "General",
            //    Title = "Portal Administration",
            //    Roles = new List<string>() { CoreServices.Update.AdminRoleId },
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Admin/Portal").Id, PaneName = "Main" } },
            //    Urls = new List<string>() { "Admin/Portal" }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.PageTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "General",
            //    Title = "Search Administration",
            //    Roles = new List<string>() { CoreServices.Update.AdminRoleId },
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Admin/Search").Id, PaneName = "Main" } },
            //    Urls = new List<string>() { "Admin/Search" }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.PageTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "General",
            //    Title = "Search Results",
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/SearchResult").Id, PaneName = "Main" } },
            //    Urls = new List<string>() { "search" }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.Menu()
            //{
            //    Type = CoreModels.Menu.MenuType.DropDown,
            //    PortalId = portalId,
            //    Name = "MainMenu",
            //    Text = "Videre",
            //    Css = "navbar-fixed-top",
            //    Items = new List<CoreModels.MenuItem>()
            //    {
            //        new CoreModels.MenuItem() {  Text = "Home", Url= "~/" },
            //        new CoreModels.MenuItem() {  Text = "Admin", AlignRight = true, Roles = new List<string>() { CoreServices.Update.AdminRoleId }, Items = new List<CoreModels.MenuItem>()
            //        {
            //            new CoreModels.MenuItem() { Text = "Portal", Url = "~/Admin/Portal" },
            //            new CoreModels.MenuItem() { Text = "Localization", Url = "~/Admin/Localization" },
            //            new CoreModels.MenuItem() { Text = "File", Url = "~/Admin/File" },
            //            new CoreModels.MenuItem() { Text = "Page Template", Url = "~/Admin/PageTemplate" },
            //            new CoreModels.MenuItem() { Text = "Layout Template", Url = "~/Admin/LayoutTemplate" },
            //            new CoreModels.MenuItem() { Text = "Security", Url = "~/Admin/Security" },
            //            new CoreModels.MenuItem() { Text = "User", Url = "~/Admin/User" },
            //            new CoreModels.MenuItem() { Text = "Widget", Url = "~/Admin/Widget" },
            //            new CoreModels.MenuItem() { Text = "Search", Url = "~/Admin/Search" }
            //        }},
            //        new CoreModels.MenuItem() {  Text = "Account", AlignRight = true, Items = new List<CoreModels.MenuItem>()
            //        {
            //            new CoreModels.MenuItem() { Text = "User Profile", Url = "~/Account/UserProfile", Authenticated = true },
            //            new CoreModels.MenuItem() { Text = "Log On", Url = "~/Account/LogOn", Authenticated = false },
            //            new CoreModels.MenuItem() { Text = "Log Off", Url = "~/core/Account/LogOff", Authenticated = true }
            //        }}
            //    }
            //});

            //var menu = CoreServices.Menu.Get(portalId, "MainMenu");

            ////todo: move out into own layout project?
            //updates += CoreServices.Update.Register(new CoreModels.LayoutTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "Blank",
            //    Widgets = new List<CoreModels.Widget>() { }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.LayoutTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "General",
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Menu").Id, PaneName = "Main", ContentIds = new List<string>() { menu.Id } } }
            //});

            //updates += CoreServices.Update.Register(new CoreModels.LayoutTemplate()
            //{
            //    PortalId = portalId,
            //    LayoutName = "TwoPane",
            //    Widgets = new List<CoreModels.Widget>() { new CoreModels.Widget() { ManifestId = CoreServices.Portal.GetWidgetManifest("Core/Menu").Id, PaneName = "Main", ContentIds = new List<string>() { menu.Id } } }
            //});
            return updates;        
        
        }
    }
}