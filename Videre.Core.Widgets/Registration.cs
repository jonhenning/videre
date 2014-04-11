using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Videre.Core.Models;
using System.Web.Mvc;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;
using System.Web.Routing;
using Videre.Core.ImportExportProviders;
using Videre.Core.Widgets.ImportExportProviders;

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
                new string[] { "Videre.Core.Widgets.Controllers" }
            );

            RouteTable.Routes.MapRoute(
                "Core_default",
                "Core/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                null,
                new string[] { "Videre.Core.Widgets.Controllers" }
            );


            //App init (for now... eventually may add PortalId)
            var updates = CoreServices.Update.Register(new CoreModels.AttributeDefinition()
            {
                GroupName = "Core",
                Name = "TextEditor",
                Values = new List<string>() { "" },
                DefaultValue = "",
                Required = true,
                LabelKey = "TextEditor.Text",
                LabelText = "Text Editor"
            });

            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition()
            {
                GroupName = "Core",
                Name = "ThemeAPIUrl",
                DefaultValue = "http://api.bootswatch.com/3",
                Required = true,
                LabelKey = "ThemeAPIUrl.Text",
                LabelText = "Theme API Url"
            });

            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition()
            {
                GroupName = "Core",
                Name = "RemotePackageUrl",
                DefaultValue = "",
                Required = false,
                LabelKey = "RemotePackageUrl.Text",
                LabelText = "Remote Package Url"
            });

            CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Core", Name = "SearchIndexDir", DefaultValue = "~/App_Data/SearchIndex", Required = true, LabelKey = "SearchIndexDir.Text", LabelText = "Search Index Directory" });
            CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Core", Name = "SearchUrl", DefaultValue = "~/search", Required = true, LabelKey = "SearchUrl.Text", LabelText = "Search Url" });

            //App init
            updates += CoreServices.Update.Register(new List<CoreModels.WidgetManifest>()
            {
                new CoreModels.WidgetManifest() { Path = "Core/Account", Name = "LogOn", Title = "Log On", Category = "Account" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Account", Name = "UserProfile", Title = "User Profile", Category = "Account" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "Localization", Title = "Localization Admin", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "Template", Title = "Template Admin", Category = "Admin" }, 
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
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "WebReference", Title = "Web Reference Admin", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "Package", Title = "Package", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "PackageAdmin", Title = "Package Admin", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Admin", Name = "PackageExport", Title = "Package Export", Category = "Admin" }, 
                new CoreModels.WidgetManifest() { Path = "Core", Name = "Menu", Title = "Menu", EditorPath = "Widgets/Core/MenuEditor", EditorType = "videre.widgets.editor.menu", ContentProvider = "Videre.Core.Widgets.ContentProviders.MenuContentProvider, Videre.Core.Widgets", Category = "Navigation" , AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition()
                    {
                        Name = "AlwaysOnTop",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "Yes",
                        Required = false,
                        LabelKey = "AlwaysOnTop.Text",
                        LabelText = "Always On Top",
                        ControlType = "bootstrap-select"
                    },
                    new AttributeDefinition()
                    {
                        Name = "InverseColors",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "No",
                        Required = false,
                        LabelKey = "InverseColors.Text",
                        LabelText = "Inverse Colors",
                        ControlType = "bootstrap-select"
                    },
                    new AttributeDefinition()
                    {
                        Name = "ShowLogo",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "No",
                        Required = false,
                        LabelKey = "ShowLogo.Text",
                        LabelText = "Show Logo",
                        ControlType = "bootstrap-select"
                    },
                    new AttributeDefinition()
                    {
                        Name = "ShowSearch",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "No",
                        Required = false,
                        LabelKey = "ShowSearch.Text",
                        LabelText = "Show Search",
                        ControlType = "bootstrap-select"
                    },
                    new AttributeDefinition()
                    {
                        Name = "Animate",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "No",
                        Required = false,
                        LabelKey = "Animate.Text",
                        LabelText = "Animate",
                        ControlType = "bootstrap-select"
                    }
                }},
                new CoreModels.WidgetManifest() { Path = "Core", Name = "SideNav", Title = "SideNav", EditorPath = "Widgets/Core/MenuEditor", EditorType = "videre.widgets.editor.menu", ContentProvider = "Videre.Core.Widgets.ContentProviders.MenuContentProvider, Videre.Core.Widgets", Category = "Navigation" , AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition()
                    {
                        Name = "Position",
                        Values = new List<string>() { "", "Fixed Left", "Fixed Right" },
                        DefaultValue = "",
                        Required = false,
                        LabelKey = "Position.Text",
                        LabelText = "Position"
                    }
                }},

                new CoreModels.WidgetManifest() { Path = "Core", Name = "TextHtml", Title = "Text Html", EditorPath = "Widgets/Core/TextHtmlEditor", EditorType = "videre.widgets.editor.texthtml", ContentProvider = "Videre.Core.ContentProviders.LocalizationContentProvider, Videre.Core", Category = "General"
                },
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
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Portal", Name = "Administration", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "PageTemplate", Name = "Administration", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "LayoutTemplate", Name = "Administration", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Localization", Name = "Administration", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "File", Name = "Administration", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "File", Name = "Upload", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Account", Name = "Administration", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Content", Name = "Administration", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Comment", Name = "Administration", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Search", Name = "Upload", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} }
            });

            updates += CoreServices.Update.Register(Services.Web.GetDefaultWebReferences(portalId));

            return updates;        
        
        }
    }
}