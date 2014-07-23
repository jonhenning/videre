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
    public class RoleImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Role"; } }
        public List<string> ProviderDependencies
        {
            get { return new List<string>(); }
        }

        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Account.GetRoles(portalId).Select(u =>
                new ImportExportContent()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Type = Name,
                    Included = (export != null && export.Roles != null ? export.Roles.Exists(u2 => u2.Id == u.Id) : false)//,
                    //Preview = 
                }).ToList();
        }
        public PortalExport Export(string id, PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            export = export ?? Services.ImportExport.GetPortalExport(portalId);
            export.Roles = export.Roles ?? new List<Models.Role>();
            var u = Services.Account.GetRoleById(id);
            if (u != null)
                export.Roles.Add(u);

            return export;
        }

        public void Import(PortalExport export, Dictionary<string, string> idMap, string portalId)
        {
            if (export.Roles != null)
            {
                Logging.Logger.DebugFormat("Importing {0} roles...", export.Roles.Count);
                foreach (var role in export.Roles)
                    ImportExport.SetIdMap<Role>(role.Id, Import(portalId, role), idMap);
            }
        }

        private string Import(string portalId, Models.Role role, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Account.GetRole(role.Name, portalId);
            role.PortalId = portalId;
            role.Id = existing != null ? existing.Id : null;
            return Account.SaveRole(role, userId);
        }

    }
}
