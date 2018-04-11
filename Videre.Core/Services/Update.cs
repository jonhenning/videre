﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Caching;
using CodeEndeavors.Extensions;
//using ICSharpCode.SharpZipLib.Zip;
using System.Web.Mvc;
using CoreServices = Videre.Core.Services;
using PaniciSoftware.FastTemplate.Common;
using Videre.Core.Extensions;

namespace Videre.Core.Services
{
    public class Update
    {
        private static Dictionary<string, string> _adminRoleIdDict = new Dictionary<string, string>();
        public static string GetAdminRoleId(string portalId = null)
        {
            portalId = !string.IsNullOrEmpty(portalId) ? portalId : Services.Portal.CurrentPortalId;
            if (!_adminRoleIdDict.ContainsKey(portalId))
            {
                var updates = Register(new List<Models.Role>()
                {
                    new Models.Role() { Name = "admin", PortalId = portalId, Description = "Administrative Priviledges" }
                });

                if (updates > 0)
                    Services.Repository.SaveChanges();

                _adminRoleIdDict[portalId] = Services.Account.GetRole("admin", portalId).Id;
            }
            return _adminRoleIdDict[portalId];
        }

        public static string UpdateDir
        {
            get
            {
                return Portal.ResolvePath(Portal.GetAppSetting("UpdateDir", "~/_updates/"));
            }
        }

        public static bool ApplyingUpdates { get; set; }
        public static bool ApplicationShutdown { get; set; }

        //public static string PackageDir
        //{
        //    get
        //    {
        //        return Portal.ResolvePath(ConfigurationManager.AppSettings.GetSetting("PackageDir", "~/_packages/"));
        //    }
        //}

        public static int Register(Models.Portal portal)
        {
            var count = 0;
            if (!Portal.Exists(portal))
            {
                Logging.Logger.InfoFormat("Registering portal: {0}", portal.Name);
                Portal.Save(portal, Account.AuditId);
                count++;
            }
            return count;
        }

        public static int Register(List<Models.WidgetManifest> manifests)
        {
            var count = 0;
            foreach (var manifest in manifests)
            {
                if (!Widget.Exists(manifest))
                {
                    Logging.Logger.InfoFormat("Registering manifest: {0}", manifest.FullName);
                    Widget.Save(manifest, Account.AuditId);
                    count++;
                }
                else
                {
                    //todo: REALLY need to reconsider how to structure this logic...  
                    var m = Widget.GetWidgetManifest(manifest.FullName);
                    manifest.Id = m.Id;
                    if (manifest.ToJson() != m.ToJson())
                    {
                        Logging.Logger.InfoFormat("Registering manifest: {0}", manifest.FullName);
                        Widget.Save(manifest, Account.AuditId);
                        count++;
                    }

                }
            }
            //Repository.SaveChanges();
            return count;
        }

        public static int Register(Models.SearchProvider provider)
        {
            var count = 0;
            if (!Search.Exists(provider))
            {
                Logging.Logger.InfoFormat("Registering search provider: {0}", provider.Name);
                Search.Save(provider, Account.AuditId);
                count++;
            }
            return count;
        }

        public static int Register(Models.PageTemplate template)
        {
            return Register(new List<Models.PageTemplate>() { template });
        }
        public static int Register(List<Models.PageTemplate> templates)
        {
            var count = 0;
            foreach (var template in templates)
            {
                template.PortalId = string.IsNullOrEmpty(template.PortalId) ? Services.Portal.CurrentPortalId : template.PortalId;
                if (!Portal.Exists(template))
                {
                    Logging.Logger.InfoFormat("Registering page template: {0}", template.Urls.ToJson());
                    Portal.Save(template, Account.AuditId);
                    count++;
                }
            }
            return count;
        }

        public static int Register(Models.LayoutTemplate template)
        {
            return Register(new List<Models.LayoutTemplate>() { template });
        }
        public static int Register(List<Models.LayoutTemplate> templates)
        {
            var count = 0;
            foreach (var template in templates)
            {
                template.PortalId = string.IsNullOrEmpty(template.PortalId) ? Services.Portal.CurrentPortalId : template.PortalId;
                if (!Portal.Exists(template))
                {
                    Logging.Logger.InfoFormat("Registering layout template: {0}", template.LayoutName);
                    Portal.Save(template, Account.AuditId);
                    count++;
                }
            }
            return count;
        }

        public static int Register(Models.Menu menu)
        {
            var count = 0;
            if (!Menu.Exists(menu))
            {
                Logging.Logger.InfoFormat("Registering menu: {0}", menu.Name);
                Menu.Save(menu, Account.AuditId);
                count++;
            }
            return count;
        }

        public static int Register(Models.Role role)
        {
            return Register(new List<Models.Role>() { role });
        }
        public static int Register(List<Models.Role> roles)
        {
            var count = 0;
            foreach (var role in roles)
            {
                if (!Account.Exists(role))
                {
                    Logging.Logger.InfoFormat("Registering role: {0}", role.Name);
                    Account.SaveRole(role, Account.AuditId);
                    count++;
                }
            }
            if (count > 0)
                Portal.SetRequestContextData<List<Models.Role>>("PortalRoles-" + roles.Select(r => r.PortalId).FirstOrDefault(), null); //clear request context for roles as we just saved a new one
            return count;
        }

        public static int Register(Models.User user)
        {
            return Register(new List<Models.User>() { user });
        }
        public static int Register(List<Models.User> users)
        {
            var count = 0;
            foreach (var user in users)
            {
                if (!Account.Exists(user) || !string.IsNullOrEmpty(user.Id))    //allowing user to be created by login, this is an update then
                {
                    Logging.Logger.InfoFormat("Registering user: {0}", user.Name);
                    Account.SaveUser(user, Account.AuditId);
                    count++;
                }
            }
            return count;
        }

        public static int Register(Models.SecureActivity activity)
        {
            return Register(new List<Models.SecureActivity>() { activity });
        }
        public static int Register(List<Models.SecureActivity> activities)
        {
            var count = 0;
            foreach (var activity in activities)
            {
                if (!Security.Exists(activity))
                {
                    Logging.Logger.InfoFormat("Registering secure activity: {0}.{1}", activity.Area, activity.Name);
                    Security.Save(activity, Account.AuditId);
                    count++;
                }
            }
            return count;
        }

        [Obsolete("Use Register(attribute) instead.  GroupName now on Attribute Model")]
        public static int Register(string groupName, Models.AttributeDefinition attribute)
        {
            attribute.GroupName = groupName;
            return Register(attribute);
        }

        public static int Register(Models.AttributeDefinition attribute)
        {
            var portals = Services.Portal.GetPortals();
            var count = 0;
            count += Services.Portal.RegisterPortalAttribute(attribute) ? 1 : 0;
            return count;
        }

        public static int Register(Models.WebReference reference)
        {
            return Register(new List<Models.WebReference>() { reference });
        }
        public static int Register(List<Models.WebReference> references)
        {
            var count = 0;
            foreach (var reference in references)
            {
                if (!Web.Exists(reference))
                {
                    Logging.Logger.InfoFormat("Registering web reference: {0}", reference.Name);
                    Web.Save(reference, Account.AuditId);
                    count++;
                }
            }
            return count;
        }

        public static void WatchForUpdates()
        {
            Logging.Logger.DebugFormat("Watching folder {0} for updates", UpdateDir);
            ApplyUpdates();
            //using cache dependency to easily monitor update folder for changes
            HttpRuntime.Cache.Add("_updates", "", new CacheDependency(UpdateDir), Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1), CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(OnFolderChanged));
        }

        private static void OnFolderChanged(string key, object value, CacheItemRemovedReason reason)
        {
            Logging.Logger.InfoFormat("Detected new file in update folder: {0} - {1} - {2}", UpdateDir, reason, value);
            HandleFolderChanged(false);
        }

        private static void HandleFolderChanged(bool afterErrorTry)
        {
            HttpRuntime.Cache.Add("_updates", "", new CacheDependency(UpdateDir), Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1), CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(OnFolderChanged));
            try
            {
                ApplyUpdates();
            }
            catch (Exception ex)
            {
                Logging.Logger.DebugFormat("Error applying update {0}", ex.Message);
                if (!afterErrorTry)
                {
                    System.Threading.Thread.Sleep(500); //try one more time
                    HandleFolderChanged(true);
                }
            }
            Videre.Core.Services.Repository.Dispose();  //need to SaveChanges as the Global.asax EndRequest not fired (we have no request)
        }

        public static int ApplyUpdates()
        {
            var updateDir = UpdateDir; //Portal.ResolvePath(ConfigurationManager.AppSettings.GetSetting("UpdateDir", "~/_updates"));
            var rootDir = Portal.ResolvePath("~/");
            if (!Directory.Exists(updateDir))
                Directory.CreateDirectory(updateDir);
            var dir = new DirectoryInfo(updateDir);
            var count = 0;
            var files = dir.GetFiles("*.zip");

            if (files.Length > 0 && !ApplyingUpdates)
            {
                ApplyingUpdates = true; //set flag to allow other code to short-circuit 
                Logging.Logger.InfoFormat("ApplyUpdates for files {0}", files.Select(f => f.Name).ToJson());
                SetAppOffline();
                foreach (var file in files)
                    count += Package.InstallFile(file.FullName) ? 1 : 0;

                if (!ApplicationShutdown)   //force recycle, as we are not running registration logic during install and instally may not copy things to bin folder
                {
                    Logging.Logger.InfoFormat("Forcing Recycle due to install");
                    HttpRuntime.UnloadAppDomain();
                }
                RemoveAppOffline();
                ApplyingUpdates = false;
            }
            //These are specific to a portal!
            //files = dir.GetFiles("*.json");
            //foreach (var file in files)
            //    count += InstallFile(file.FullName) ? 1 : 0;
            return count;
        }

        public static void SetAppOffline()
        {
            var rootDir = Portal.ResolvePath("~/");
            var fileName = Path.Combine(rootDir, "app_offline.txt");
            if (System.IO.File.Exists(fileName))
            {
                Logging.Logger.Info("Setting App Offline");
                System.IO.File.Copy(fileName, Path.Combine(rootDir, "app_offline.htm"));
            }
        }

        public static void RemoveAppOffline()
        {
            try
            {
                var rootDir = Portal.ResolvePath("~/");
                var fileName = Path.Combine(rootDir, "app_offline.htm");
                if (System.IO.File.Exists(fileName))
                {
                    Logging.Logger.Info("Removing App Offline");
                    System.IO.File.Delete(fileName);
                }
            }
            catch (Exception ex)
            {
                Logging.Logger.Error(ex.Message);
                throw;
            }
        }

        //todo: allowing passing in of objects, but currently using very little of them
        public static string InstallPortal(Core.Models.User adminUser, Core.Models.Portal portal, string authenticationProvider)
        {
            try
            {
                //todo: hardcoding default for now...
                //if (Core.Services.Update.Register(new Videre.Core.Models.Portal() { Name = portal.Name, ThemeName = portal.ThemeName }) > 0)
                if (Core.Services.Update.Register(portal) > 0)
                    Core.Services.Repository.SaveChanges();

                var portalId = Core.Services.Portal.GetPortal(portal.Name).Id; //Core.Services.Portal.CurrentPortalId;
                var updates = 0;

                if (CoreServices.Portal.CurrentPortal == null)
                    Caching.RemoveRequestCacheEntry("CurrentPortal");

                //portal Init
                updates += Core.Services.Update.Register(new List<Core.Models.Role>()
                {
                    new Core.Models.Role() { Name = "admin", PortalId = portalId, Description = "Administrative Priviledges" }
                });


                var newAdminUser = new Core.Models.User() { PortalId = portalId, Name = adminUser.Name, Email = adminUser.Email, Password = adminUser.Password, RoleIds = new List<string>() { Core.Services.Update.GetAdminRoleId(portalId) } };
                if (Authentication.PersistenceProvider == null) //user must exist in authentication and we need to add the admin role to it.
                {
                    if (!string.IsNullOrEmpty(authenticationProvider))
                    {
                        CoreServices.Portal.CurrentPortal.Attributes["Authentication." + authenticationProvider + "-Options"] = new Newtonsoft.Json.Linq.JArray() { "Allow Creation", "Allow Login" };
                        CoreServices.Portal.Save(CoreServices.Portal.CurrentPortal);

                        var loginResult = Authentication.Login(adminUser.Name, adminUser.Password, false, authenticationProvider);
                        if (!string.IsNullOrEmpty(loginResult.UserId))
                        {
                            newAdminUser = CoreServices.Account.GetUserById(loginResult.UserId);    //need to re-get user as it now has claims (AuthenticationToken)
                            newAdminUser.RoleIds = new List<string>() { Core.Services.Update.GetAdminRoleId(portalId) };
                            CoreServices.Authentication.RevokeAuthenticationTicket();   //don't remain authenticated as we wouldn't have admin rights.
                        }
                        else
                            throw new Exception("Invalid Login for user.  User must already exist in one of the standard login providers.");
                    }
                    else
                    {
                        var userAuthenticated = false;
                        //todo:  try each one?   seems a bit much, but user is not able to configure them yet...
                        foreach (var provider in Authentication.GetStandardAuthenticationProviders())
                        {
                            var result = Authentication.Authenticate(adminUser.Name, adminUser.Password, provider.Name);
                            if (result.Success)
                            {
                                newAdminUser.Password = null; //password handled by outside provider
                                userAuthenticated = true;
                                break;
                            }
                        }
                        if (!userAuthenticated)
                            throw new Exception("Invalid Login for user.  User must already exist in one of the standard login providers.");
                    }
                }
                updates += Core.Services.Update.Register(new List<Core.Models.User>() { newAdminUser });

                if (updates > 0)
                    CoreServices.Repository.SaveChanges();

                SendWelcomeEmail(adminUser, portal);

                return portalId;
            }
            catch (Exception ex)
            {
                Update.ResetAllPortals();
                throw ex;
            }
        }

        public static void SendWelcomeEmail(Core.Models.User adminUser, Core.Models.Portal portal)
        {
            var subject = Localization.GetPortalText("PortalEmailWelcomeSubject.Text", "Welcome $UserName, your new videre portal ($PortalName) has been created!");
            var body = Localization.GetPortalText("PortalEmailWelcomeBody.Text", "<p>Welcome <b>$UserName</b>, your new videre portal ($PortalName) has been created!</p>");
            var tokens = new Dictionary<string, object>()
                {
                    {"UserName", adminUser.Name},
                    {"UserEmail", adminUser.Email},
                    {"PortalName", portal.Name}
                };
            Mail.Send(adminUser.Email, adminUser.Email, "WelcomeEmail", subject, body, tokens);
        }

        public static event EventHandler PreRegister;
        public static event EventHandler PostRegister;
        public static void Register()
        {
            PreRegister(null, new EventArgs());
            //if (Services.Portal.CurrentPortal != null)  //todo: best way to handle this?
            //{
            //    if (Services.Update.Register(new Videre.Core.Models.Portal() { Name = "Default" }) > 0)
            //        Services.Repository.SaveChanges();
            //    Services.Update.RegisterWidgets();
            //}
            Services.Update.RegisterWidgets();
            //Services.Update.RegisterImportExportProviders();
            Services.Authentication.RegisterAuthenticationProviders();
            Services.Authentication.RegisterAuthenticationResetProviders();
            Services.WebReferenceBundler.RegisterWebReferenceBundlers();

            PostRegister(null, new EventArgs());
        }

        public static void RegisterWidgets()
        {
            var widgetRegistrations = ReflectionExtensions.GetAllInstances<Models.IWidgetRegistration>();

            var updates = 0;
            foreach (var registration in widgetRegistrations)
            {
                if (!ApplyingUpdates)
                {
                    using (new Videre.Core.Services.Profiler.Timer("Registering Widget: " + registration.GetType().ToString(), true))
                    {
                        Logging.Logger.InfoFormat("Registering {0}", registration.GetType().ToString());
                        //var w = (Models.IWidgetRegistration)ObjectFactory.GetInstance(registration.PluginType);
                        updates += registration.Register();
                    }
                }
                else
                    Logging.Logger.InfoFormat("Not Registering Widget {0}.  Applying Updates", registration.GetType().ToString());
            }
            if (!ApplyingUpdates)
                updates += RegisterPortals(false);
            else
                Logging.Logger.InfoFormat("Not Registering Portals.  Applying Updates");

            if (updates > 0)
                CoreServices.Repository.SaveChanges();

        }

        [Obsolete()]
        public static void RegisterImportExportProviders()
        {
            //var providers = ReflectionExtensions.GetAllInstances<ImportExportProviders.IImportExportProvider>();

            //Logging.Logger.InfoFormat("Found {0} import/export providers - {1}", providers.Count, providers.ToJson());
            //foreach (var provider in providers)
            //    Services.ImportExport.RegisterProvider(provider);
            //Logging.Logger.DebugFormat("Registered import/export providers: {0} ({1})", providers.Count, providers.ToJson());

        }

        public static int RegisterPortals(bool persist = true)
        {
            var widgetRegistrations = ReflectionExtensions.GetAllInstances<Models.IWidgetRegistration>();

            var updates = 0;
            var portals = Services.Portal.GetPortals();
            foreach (var portal in portals)
            {
                foreach (var registration in widgetRegistrations)
                {
                    Logging.Logger.InfoFormat("Registering {0} on portal {1}", registration.GetType().ToString(), portal.Id);
                    updates += registration.RegisterPortal(portal.Id);
                }
            }
            if (persist && updates > 0)
                CoreServices.Repository.SaveChanges();
            return updates;
        }

        //nuclear option!
        public static void ResetAllPortals()
        {           
            Repository.DeleteAll<Models.PageTemplate>("PageTemplate");
            Repository.DeleteAll<Models.LayoutTemplate>("LayoutTemplate");
            Repository.DeleteAll<Models.Menu>("Menu");
            Repository.DeleteAll<Models.Package>("Package");
            Repository.DeleteAll<Models.Localization>("Localization");
            Repository.DeleteAll<Models.Portal>("Portal");
            Repository.DeleteAll<Models.Role>("Role");
            Repository.DeleteAll<Models.SearchProvider>("SearchProvider");
            Repository.DeleteAll<Models.SecureActivity>("SecureActivity");
            Repository.DeleteAll<Models.User>("User");
            Repository.DeleteAll<Models.WebReference>("WebReference");
            Repository.DeleteAll<Models.WidgetManifest>("WidgetManifest");
            CoreServices.Repository.PendingUpdates = 0;

        }
    }
}