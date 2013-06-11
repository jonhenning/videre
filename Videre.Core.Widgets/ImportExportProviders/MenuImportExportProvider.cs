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
    public class MenuImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Menu"; } }
        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Menu.Get(portalId).Select(m =>
                new ImportExportContent()
                {
                    Id = m.Id,
                    Name = m.Name,
                    Included = (export != null && export.Menus != null ? export.Menus.Exists(m2 => m2.Id == m.Id) : false),
                    Preview = m.Text
                }).ToList();
        }
        public PortalExport Export(string id, PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            export = export ?? Services.ImportExport.GetPortalExport(portalId);
            export.Menus = export.Menus ?? new List<Models.Menu>();
            var r = Services.Menu.GetById(id);
            if (r != null)
                export.Menus.Add(r);
            return export;
        }

    }
}
