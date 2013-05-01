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
    public class LocalizationImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Localization"; } }
        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Localization.Get(portalId).Select(l =>
                new ImportExportContent()
                {
                    Id = l.Id,
                    Name = l.Namespace + "/" + l.Key,
                    Included = (export != null && export.Localizations != null ? export.Localizations.Exists(l2 => l2.Id == l.Id) : false),
                    Preview = l.Text
                }).ToList();
        }
        public PortalExport Export(string id, PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            export = export ?? Services.ImportExport.GetPortalExport(portalId);
            export.Localizations = export.Localizations ?? new List<Models.Localization>();
            var l = Services.Localization.GetById(id);
            if (l != null)
                export.Localizations.Add(l);
            return export;

        }

    }
}
