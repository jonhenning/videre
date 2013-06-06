using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Models;
using CodeEndeavors.Extensions;
using System.Collections.Concurrent;
using Videre.Core.ImportExportProviders;

namespace Videre.Core.Widgets.ImportExportProviders
{
    public class WebReferenceImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Web Reference"; } }
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

    }
}
