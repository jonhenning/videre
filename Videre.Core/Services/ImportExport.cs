using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Models;
using CodeEndeavors.Extensions;
using System.Collections.Concurrent;
using Videre.Core.ImportExportProviders;

namespace Videre.Core.Services
{
    public class ImportExport
    {
        private static ConcurrentDictionary<string, IImportExportProvider> _importExportProviders = new ConcurrentDictionary<string, IImportExportProvider>();

        public static void RegisterProvider(IImportExportProvider provider)
        {
            _importExportProviders[provider.Name] = provider;
        }

        public static IImportExportProvider GetProvider(string name)
        {
            if (_importExportProviders.ContainsKey(name))
                return _importExportProviders[name];

            throw new Exception(string.Format("Import Export Provider Type {0} not registered", name));
        }

        public static List<string> GetRegisteredProviders()
        {
            return _importExportProviders.Values.Select(p => p.Name).OrderBy(p => p).ToList();
        }

        public static bool Import(PortalExport export, string portalId)
        {
            var idMap = new Dictionary<string, string>();

            var portal = Portal.GetPortalById(portalId);
            if (portal != null)
            {
                export.Portal.Id = portal.Id;
                export.Portal.Name = portal.Name;
                //IMPORTANT:  since we pass in the portalId that we want to import into, we need to ensure that is used, therefore, the name must match as that is how the duplicate lookup is done.  (i.e. {Id: 1, Name=Default} {Id: 2, Name=Foo}.  If we import Id:2 and its name importing is Default)
                export.Portal.Default = portal.Default;

                //if export does not contain these values, revert back to originals
                if (export.Portal.AdministratorEmail == null)
                    export.Portal.AdministratorEmail = portal.AdministratorEmail;
                if (export.Portal.ThemeName == null)
                    export.Portal.ThemeName = portal.ThemeName;
                if (export.Portal.LogoUrl == null)
                    export.Portal.LogoUrl = portal.LogoUrl;
                if (export.Portal.Title == null)
                    export.Portal.Title = portal.Title;
                if (export.Portal.Aliases == null)
                    export.Portal.Aliases = portal.Aliases;

                if (export.Portal.Attributes == null)
                    export.Portal.Attributes = new Dictionary<string, object>();

                export.Portal.Attributes = export.Portal.Attributes.Merge(portal.Attributes);
            }
            else
                throw new Exception("Portal Not found: " + portalId);

            Portal.Save(export.Portal);
            portal = Portal.GetPortal(export.Portal.Name);
            SetIdMap<Models.Portal>(portal.Id, export.Portal.Id, idMap);

            var finishedProviders = new List<string>();
            foreach (var providerName in GetRegisteredProviders())
                Import(providerName, export, idMap, portal.Id, finishedProviders);

            return true;
        }

        private static void Import(string name, PortalExport export, Dictionary<string, string> idMap, string portalId, List<string> finishedProviders)
        {
            if (!finishedProviders.Contains(name))
            {
                var provider = GetProvider(name);
                foreach (var depName in provider.ProviderDependencies)
                    Import(depName, export, idMap, portalId, finishedProviders);
                GetProvider(name).Import(export, idMap, portalId);
                finishedProviders.Add(name);
            }
        }

        public static string SetIdMap<T>(string id, string value, Dictionary<string, string> map)
        {
            var key = string.Format("{0}~{1}", typeof(T), id);
            return map[key] = value;
        }

        public static string GetIdMap<T>(string id, Dictionary<string, string> map)
        {
            var key = string.Format("{0}~{1}", typeof(T), id);
            return map.GetSetting<string>(key, null);
        }

        public static PortalExport GetPortalExport(string portalId)
        {
            var export = new PortalExport();
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            export.Portal = Portal.GetPortalById(portalId);
            return export;
        }

    }
}
