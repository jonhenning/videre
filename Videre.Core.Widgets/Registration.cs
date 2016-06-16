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
            var updates = 0;

            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Core", Name = "ThemeAPIUrl", DefaultValue = "http://api.bootswatch.com/3", Required = true, LabelKey = "ThemeAPIUrl.Text", LabelText = "Theme API Url" });
            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Core", Name = "RemotePackageUrl", DefaultValue = "",  Required = false, LabelKey = "RemotePackageUrl.Text", LabelText = "Remote Package Url" });
            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Core", Name = "SearchIndexDir", DefaultValue = "~/App_Data/SearchIndex", Required = true, LabelKey = "SearchIndexDir.Text", LabelText = "Search Index Directory" });
            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Core", Name = "SearchUrl", DefaultValue = "~/search", Required = true, LabelKey = "SearchUrl.Text", LabelText = "Search Url" });

            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Account", Name = "AccountVerificationUrl", DefaultValue = "~/account/verify", Required = false, LabelKey = "AccountVerificationUrl.Text", LabelText = "Account Verification Url", TooltipKey = "AccountVerificationUrlTooltip.Text", TooltipText = "Url for page template that contains widget responsible for verifying account.  If this is not set, then Account Verification Mode is ignored." });
            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Account", Name = "CreateAccountUrl", DefaultValue = "~/account/create", Required = false, LabelKey = "AccountCreationUrl.Text", LabelText = "Account Creation Url", TooltipKey = "AccountCreationUrlTooltip.Text", TooltipText = "Url for page template that contains widget responsible for creating account.  If this is not set, then Account Creation specified on the account providers is ignored." });
            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Account", Name = "CreateAccountSuccessUrl", DefaultValue = "", Required = false, LabelKey = "AccountCreationSuccessUrl.Text", LabelText = "Account Create Success Url", TooltipKey = "AccountCreateSuccessUrlTooltip.Text", TooltipText = "Url for page template that contains information to display after successful account creation.  If this is not set then will function like login." });
            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Account", Name = "AccountVerificationMode", Values = new List<string>() { "None", "Passive", "Enforced" }, DefaultValue = "None", Required = false, LabelKey = "AccountVerificationMode.Text", LabelText = "Account Verification Mode", TooltipKey = "AccountVerificationModeTooltip.Text", TooltipText = "<b>None</b> - No verification.<br/><b>Passive</b> - During login, check to see if verified, if not redirect.  User may skip verification from there.<br/><b>Enforced</b> - User is forced to enter verification.  An authenticated user who is not verified will be forced to Verify page." });
            
            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Account", Name = "AccountProvider", DefaultValue = CoreServices.Account.AccountService.GetType().FullName + ", " + CoreServices.Account.AccountService.GetType().Assembly.GetName().Name, Required = false, LabelKey = "AccountProvider.Text", LabelText = "Account Provider", TooltipKey = "AccountProviderTooltip.Text", TooltipText = "Provider used to manage account profile information. The Account Provider can only be configured in the app.config.", ReadOnly = true });

            //temporarly cleanup... remove in future
            updates += CoreServices.Portal.UnregisterPortalAttribute("Authentication", "AccountVerificationUrl") ? 1 : 0;
            updates += CoreServices.Portal.UnregisterPortalAttribute("Authentication", "CreateAccountUrl") ? 1 : 0;
            updates += CoreServices.Portal.UnregisterPortalAttribute("Authentication", "CreateAccountSuccessUrl") ? 1 : 0;
            updates += CoreServices.Portal.UnregisterPortalAttribute("Authentication", "AccountVerificationMode") ? 1 : 0;

            //var providers = new List<string>() { "" };
            //providers.AddRange(Services.Authentication.GetAuthenticationPersistenceProviders().Select(p => p.Name));
            //updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = "PersistenceProvider", Values = providers, DefaultValue = "", Required = false, LabelKey = "AuthenticationPersistenceProvider.Text", LabelText = "Authentication Persistence Provider", TooltipKey = "PersistenceProviderTooltip.Text", TooltipText = "Provider used to store user authentication credentials.  It is configured in the app.config", ReadOnly = true });
            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = "PersistenceProvider", DefaultValue = CoreServices.Authentication.PersistenceProvider != null ? (CoreServices.Authentication.PersistenceProvider.GetType().Name + ", " + CoreServices.Authentication.PersistenceProvider.GetType().Assembly.GetName().Name) : "", Required = false, LabelKey = "AuthenticationPersistenceProvider.Text", LabelText = "Authentication Persistence Provider", TooltipKey = "PersistenceProviderTooltip.Text", TooltipText = "Provider used to store user authentication credentials.  It is configured in the app.config", ReadOnly = true });

            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = "PasswordExpiresDays", DefaultValue = null, DataType = "number", Required = false, LabelKey = "PasswordExpiresDays.Text", LabelText = "Password Expires (days)", TooltipKey = "PasswordExpiresDaysTooltip.Text", TooltipText = "Days until password expires (leave blank for no expiration)" });
            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = "EnableImpersonation", Values = new List<string>() { "Yes", "No" }, DefaultValue = "No", Required = false, LabelKey = "EnableImpersonation.Text", LabelText = "Enable Impersonation" });
            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition() { GroupName = "Authentication", Name = "EnableTwoPhaseAuthentication", Values = new List<string>() { "Yes", "No" }, DefaultValue = "No", Required = false, LabelKey = "EnableTwoPhaseAuthentication.Text", LabelText = "Enable Two-Phase Authentication" });

            //unregister widgets that moved to subwidgets
            var manifest = CoreServices.Widget.GetWidgetManifest("Core/Admin/PackageAdmin");
            if (manifest != null)
                CoreServices.Widget.DeleteManifest(manifest.Id);
            manifest = CoreServices.Widget.GetWidgetManifest("Core/Admin/PackageExport");
            if (manifest != null)
                CoreServices.Widget.DeleteManifest(manifest.Id);

            //App init
            updates += CoreServices.Update.Register(new List<CoreModels.WidgetManifest>()
            {
                new CoreModels.WidgetManifest() { Path = "Core/Account", Name = "LogOn", Title = "Log On", Category = "Account", AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition()
                    {
                        Name = "ShowForgotPassword",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "Yes",
                        Required = false,
                        LabelKey = "ShowForgotPassword.Text",
                        LabelText = "Show Forgot Password",
                        ControlType = "bootstrap-select"
                    },
                    new AttributeDefinition()
                    {
                        Name = "ShowCreate",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "Yes",
                        Required = false,
                        LabelKey = "ShowCreate.Text",
                        LabelText = "Show Create",
                        ControlType = "bootstrap-select"
                    },
                    new AttributeDefinition()
                    {
                        Name = "RedirectUrl",
                        DefaultValue = "",
                        Required = false,
                        LabelKey = "RedirectUrl.Text",
                        LabelText = "Redirect Url"
                    }  
                }}, 
                new CoreModels.WidgetManifest() { Path = "Core/Account", Name = "UserProfile", Title = "User Profile", Category = "Account" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Account", Name = "Verify", Title = "Verify Account", Category = "Account" }, 
                new CoreModels.WidgetManifest() { Path = "Core/Account", Name = "ImpersonateUser", Title = "Impersonate User", Category = "Account" }, 
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
                new CoreModels.WidgetManifest() { Path = "Core", Name = "Menu", Title = "Menu", EditorPath = "Widgets/Core/MenuEditor", EditorType = "videre.widgets.editor.menu", ContentProvider = "Videre.Core.Widgets.ContentProviders.MenuContentProvider, Videre.Core.Widgets", Category = "Navigation", AttributeDefinitions = new List<AttributeDefinition>()
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
                        Name = "NavbarRight",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "No",
                        Required = false,
                        LabelKey = "NavbarRight.Text",
                        LabelText = "Navbar Right",
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

            updates += UpgradePageTemplatesToUseLayoutId(portalId);

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
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Account", Name = "Impersonation", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Content", Name = "Administration", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Comment", Name = "Administration", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Search", Name = "Upload", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} },
                new CoreModels.SecureActivity() { PortalId = portalId, Area = "Profiler", Name = "Glimpse", RoleIds = new List<string>() {CoreServices.Update.GetAdminRoleId(portalId)} }
            });

            updates += CoreServices.Update.Register(Services.Web.GetDefaultWebReferences(portalId));

            return updates;        
        
        }

        //relationship for page templates to Layout is now going to be by Id instead of Name.
        private int UpgradePageTemplatesToUseLayoutId(string portalId)
        {
            var count = 0;
            var pageTemplates = CoreServices.Portal.GetPageTemplates(portalId).Where(p => p.LayoutId == null).ToList();
            foreach (var template in pageTemplates)
            {
                var layoutTemplate = CoreServices.Portal.GetLayoutTemplate(portalId, template.LayoutName);
                if (layoutTemplate != null)
                {
                    template.LayoutId = layoutTemplate.Id;
                    count++;
                    CoreServices.Portal.Save(template);
                }
            }
            return count;
        }
        

    }
}