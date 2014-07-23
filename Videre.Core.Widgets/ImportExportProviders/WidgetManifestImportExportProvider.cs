using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Models;
using CodeEndeavors.Extensions;
using System.Collections.Concurrent;
using Videre.Core.ImportExportProviders;
using Videre.Core.Services;

namespace Videre.Core.Widgets.ImportExportProviders
{
    public class WidgetManifestImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Widget Manifest"; } }
        public List<string> ProviderDependencies
        {
            get { return new List<string>(); }
        }

        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Widget.GetWidgetManifests().Select(u =>
                new ImportExportContent()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Type = Name,
                    Included = (export != null && export.Manifests != null ? export.Manifests.Exists(u2 => u2.Id == u.Id) : false)//,
                    //Preview = 
                }).ToList();
        }
        public PortalExport Export(string id, PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            export = export ?? Services.ImportExport.GetPortalExport(portalId);
            export.Manifests = export.Manifests ?? new List<Models.WidgetManifest>();
            var u = Services.Widget.GetWidgetManifestById(id);
            if (u != null)
                export.Manifests.Add(u);

            return export;
        }

        public void Import(PortalExport export, Dictionary<string, string> idMap, string portalId)
        {
            if (export.Manifests != null)
            {
                Logging.Logger.DebugFormat("Importing {0} WidgetManifests...", export.Manifests.Count);
                foreach (var exportWidgetManifest in export.Manifests)
                    ImportExport.SetIdMap<WidgetManifest>(exportWidgetManifest.Id, Import(exportWidgetManifest), idMap);
            }
        }

        private string Import(WidgetManifest manifest, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Services.Widget.GetWidgetManifest(manifest.FullName);
            manifest.Id = existing != null ? existing.Id : null;
            return Services.Widget.Save(manifest, userId);
        }

    }
}
