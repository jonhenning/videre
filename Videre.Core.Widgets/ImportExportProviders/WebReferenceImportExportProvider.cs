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
    public class WebReferenceImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Web Reference"; } }
        public List<string> ProviderDependencies
        {
            get { return new List<string>(); }
        }
        
        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Web.GetWebReferences(portalId).Select(r =>
                new ImportExportContent()
                {
                    Id = r.Id,
                    Name = r.Name,
                    Included = (export != null && export.WebReferences != null ? export.WebReferences.Exists(r2 => r2.Id == r.Id) : false),
                    Preview = r.Url
                }).ToList();
        }
        public PortalExport Export(string id, PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            export = export ?? Services.ImportExport.GetPortalExport(portalId);
            export.WebReferences = export.WebReferences ?? new List<Models.WebReference>();
            var r = Services.Web.GetWebReferenceById(id);
            if (r != null)
                export.WebReferences.Add(r);
            return export;
        }

        public void Import(PortalExport export, Dictionary<string, string> idMap, string portalId)
        {
            if (export.WebReferences != null)
            {
                Logging.Logger.DebugFormat("Importing {0} web references...", export.WebReferences.Count);
                foreach (var exportWebReference in export.WebReferences)
                    ImportExport.SetIdMap<Models.WebReference>(exportWebReference.Id, Import(portalId, exportWebReference, idMap), idMap);
            }
        }

        private string Import(string portalId, Models.WebReference webReference, Dictionary<string, string> idMap, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Services.Web.GetWebReference(portalId, webReference.Name);
            webReference.Id = existing != null ? existing.Id : null;
            webReference.PortalId = portalId;
            return Services.Web.Save(webReference, userId);
        }

    }
}
