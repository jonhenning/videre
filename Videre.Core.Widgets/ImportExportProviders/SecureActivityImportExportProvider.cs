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
    public class SecureActivityImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Secure Activity"; } }

        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Security.GetSecureActivities(portalId: portalId).Select(a =>
                new ImportExportContent()
                {
                    Id = a.Id,
                    Name = a.Area + "/" + a.Name,
                    Included = (export != null && export.SecureActivities != null ? export.SecureActivities.Exists(a2 => a2.Id == a.Id) : false)//,
                    //Preview = 
                }).ToList();
        }
        public PortalExport Export(string id, PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            export = export ?? Services.ImportExport.GetPortalExport(portalId);
            export.SecureActivities = export.SecureActivities ?? new List<Models.SecureActivity>();
            var a = Services.Security.GetSecureActivityById(id);
            if (a != null)
            {
                export.SecureActivities.Add(a);

                export.Roles = export.Roles ?? new List<Models.Role>();
                export.Roles.AddRange(Services.Account.GetRoles(portalId).Where(r => a.Roles.Contains(r.Id)));
            }

            return export;
        }

    }
}
