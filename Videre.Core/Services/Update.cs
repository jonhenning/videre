using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Caching;
using CodeEndeavors.Extensions;
using ICSharpCode.SharpZipLib.Zip;
using StructureMap;
using System.Web.Mvc;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Services
{
    public class Update
    {
        private static Dictionary<string, string> _adminRoleIdDict = new Dictionary<string, string>();
        public static string AdminRoleId
        {
            get
            {
                if (!_adminRoleIdDict.ContainsKey(Services.Portal.CurrentPortalId))
                {
                    var updates = Register(new List<Models.Role>()
                    {
                        new Models.Role() { Name = "admin", PortalId = Services.Portal.CurrentPortalId, Description = "Administrative Priviledges" }
                    });

                    if (updates > 0)
                        Services.Repository.SaveChanges();
                    _adminRoleIdDict[Services.Portal.CurrentPortalId] = Services.Account.GetRole("admin").Id;
                }
                return _adminRoleIdDict[Services.Portal.CurrentPortalId];
            }
        }

        public static string UpdateDir
        {
            get
            {
                return Portal.ResolvePath(ConfigurationManager.AppSettings.GetSetting("UpdateDir", "~/_updates/"));
            }
        }

        public static string PackageDir
        {
            get
            {
                return Portal.ResolvePath(ConfigurationManager.AppSettings.GetSetting("PackageDir", "~/_packages/"));
            }
        }

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
                if (!Portal.Exists(manifest))
                {
                    Logging.Logger.InfoFormat("Registering manifest: {0}", manifest.FullName);
                    Portal.Save(manifest, Account.AuditId);
                    count++;
                }
            }
            //Repository.Current.SaveChanges();
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
                if (!Account.Exists(user))
                {
                    Logging.Logger.InfoFormat("Registering user: {0}", user.Name);
                    Account.SaveUser(user, Account.AuditId);
                    count++;
                }
            }
            return count;
        }

        public static int Register(Models.SecureActivity route)
        {
            return Register(new List<Models.SecureActivity>() { route });
        }
        public static int Register(List<Models.SecureActivity> routes)
        {
            var count = 0;
            foreach (var route in routes)
            {
                if (!Security.Exists(route))
                {
                    Logging.Logger.InfoFormat("Registering secure activity: {0}.{1}", route.Area, route.Name);
                    Security.Save(route, Account.AuditId);
                    count++;
                }
            }
            return count;
        }

        public static int Register(string groupName, Models.AttributeDefinition attribute)
        {
            var portals = Services.Portal.GetPortals();
            var count = 0;
            foreach (var portal in portals)
                count += Services.Portal.RegisterPortalAttribute(portal.Id, groupName, attribute) ? 1 : 0;
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
            Logging.Logger.InfoFormat("Detected new file in update folder: {0}", UpdateDir);
            HttpRuntime.Cache.Add("_updates", "", new CacheDependency(UpdateDir), Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1), CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(OnFolderChanged));
            ApplyUpdates();
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
            foreach (var file in files)
                count += InstallFile(file.FullName) ? 1 : 0;
            files = dir.GetFiles("*.json");
            foreach (var file in files)
                count += InstallFile(file.FullName) ? 1 : 0;
            return count;
        }

        //todo: allowing passing in of objects, but currently using very little of them
        public static void InstallPortal(Core.Models.User adminUser, Core.Models.Portal portal)
        {
            //todo: hardcoding default for now...
            if (Core.Services.Update.Register(new Videre.Core.Models.Portal() { Name = portal.Name, ThemeName = portal.ThemeName }) > 0)
                Core.Services.Repository.SaveChanges();

            var portalId = Core.Services.Portal.CurrentPortalId;
            var updates = 0;

            //portal Init
            updates += Core.Services.Update.Register(new List<Core.Models.Role>()
            {
                new Core.Models.Role() { Name = "admin", PortalId = portalId, Description = "Administrative Priviledges" }
            });

            if (!Core.Services.Account.ReadOnly)    //todo:  don't allow this?!?!  or does UI disable this?
            {
                updates += Core.Services.Update.Register(new List<Core.Models.User>()
                {
                    new Core.Models.User() { PortalId = portalId, Name = adminUser.Name, Email = adminUser.Email, Password = adminUser.Password, Roles = new List<string>() {Core.Services.Update.AdminRoleId} }
                });
            }
            if (updates > 0)
                CoreServices.Repository.SaveChanges();

        }

        public static void Register()
        {
            if (Services.Portal.CurrentPortal != null)  //todo: best way to handle this?
            {
                if (Services.Update.Register(new Videre.Core.Models.Portal() { Name = "Default" }) > 0)
                    Services.Repository.SaveChanges();
                Services.Update.RegisterWidgets();
            }
        }

        public static void RegisterWidgets()
        {
            //var widgetRegistrations = ObjectFactory.Model.PluginTypes.Where(x => typeof(Models.IWidgetRegistration).IsAssignableFrom(x.PluginType));

            ObjectFactory.Configure(x =>
                x.Scan(scan =>
                {
                    //scan.Assembly(assemblyName);
                    scan.AssembliesFromApplicationBaseDirectory();
                    scan.AddAllTypesOf<Models.IWidgetRegistration>();
                }));
            var widgetRegistrations = ObjectFactory.GetAllInstances<Models.IWidgetRegistration>();

            var updates = 0;
            foreach (var registration in widgetRegistrations)
            {
                Logging.Logger.InfoFormat("Registering {0}", registration.GetType().ToString());
                //var w = (Models.IWidgetRegistration)ObjectFactory.GetInstance(registration.PluginType);
                updates += registration.Register();
            }
            updates += RegisterPortals(false);
            if (updates > 0)
                CoreServices.Repository.SaveChanges();

        }

        public static int RegisterPortals(bool persist = true)
        {
            //var widgetRegistrations = ObjectFactory.Model.PluginTypes.Where(x => typeof(Models.IWidgetRegistration).IsAssignableFrom(x.PluginType));

            ObjectFactory.Configure(x =>
                x.Scan(scan =>
                {
                    //scan.Assembly(assemblyName);
                    scan.AssembliesFromApplicationBaseDirectory();
                    scan.AddAllTypesOf<Models.IWidgetRegistration>();
                }));
            var widgetRegistrations = ObjectFactory.GetAllInstances<Models.IWidgetRegistration>();

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

        public static List<Models.Package> GetPackages()
        {
            var packages = new List<Models.Package>();
            var packageDir = Portal.ResolvePath(PackageDir);
            if (!Directory.Exists(packageDir))
                Directory.CreateDirectory(packageDir);
            
            //todo: hacky as we copy logic for each file type... refactor!
            foreach (var file in Directory.GetFiles(packageDir, "*.manifest"))
            {
                var package = file.GetFileJSONObject<Models.Package>(true);
                packages.Add(package);
            }
            return packages;
        }

        public static bool InstallPackage(string name)
        {
            var package = GetPackages().Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (package != null)
            {
                InstallFile(Path.Combine(Portal.ResolvePath(PackageDir), package.FileName));
                //System.IO.File.Copy(Path.Combine(Portal.ResolvePath(PackageDir), package.FileName), Path.Combine(Portal.ResolvePath(UpdateDir), package.FileName), true);
                return true;
            }
            return false;
        }

        private static bool InstallFile(string fileName)
        {
            var rootDir = Portal.ResolvePath("~/");
            var file = new FileInfo(fileName);
            if (file != null)
            {
                switch (file.Extension.ToLower())
                {
                    case ".zip":
                        {
                            Logging.Logger.InfoFormat("Applying update for file: {0}", file.FullName);
                            var zip = new FastZip();
                            zip.ExtractZip(file.FullName, rootDir, FastZip.Overwrite.Always, null, null, null, true);
                            System.IO.File.Delete(file.FullName);
                            return true;
                        }
                    case ".json":
                        {
                            Logging.Logger.InfoFormat("Applying import for file: {0}", file.FullName);
                            var portalExport = file.FullName.GetFileJSONObject<Models.PortalExport>(false);
                            Services.Portal.Import(portalExport);
                            System.IO.File.Delete(file.FullName);
                            return true;
                        }
                    default:
                        {
                            Logging.Logger.Error("Unknown File Extension: " + file.Extension);
                            break;
                            //throw new Exception("Unknown File Extension: " + file.Extension);
                        }
                }
            }
            return false;
        }


    }
}