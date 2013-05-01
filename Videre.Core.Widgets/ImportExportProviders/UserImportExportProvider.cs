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
    public class UserImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "User"; } }

        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Account.GetUsers(portalId).Select(u =>
                new ImportExportContent()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Included = (export != null && export.Users != null ? export.Users.Exists(u2 => u2.Id == u.Id) : false)//,
                    //Preview = 
                }).ToList();
        }
        public PortalExport Export(string id, PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            export = export ?? Services.ImportExport.GetPortalExport(portalId);
            export.Users = export.Users ?? new List<Models.User>();
            var u = Services.Account.GetUserById(id);
            if (u != null)
            {
                export.Users.Add(u);
                export.Roles = export.Roles ?? new List<Models.Role>();
                export.Roles.AddRange(Services.Account.GetRoles(portalId).Where(r => u.Roles.Contains(r.Id)));
            }

            return export;
        }

    }
}
