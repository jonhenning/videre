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
    public class UserImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "User"; } }
        public List<string> ProviderDependencies
        {
            get { return new List<string>() {"Role"}; }
        }

        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Account.GetUsers(portalId).Select(u =>
                new ImportExportContent()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Type = Name,
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
                export.Roles.AddRange(Services.Account.GetRoles(portalId).Where(r => u.RoleIds.Contains(r.Id)));
            }

            return export;
        }

        public void Import(PortalExport export, Dictionary<string, string> idMap, string portalId)
        {
            if (export.Users != null)
            {
                Logging.Logger.DebugFormat("Importing {0} users...", export.Users.Count);
                foreach (var exportUser in export.Users)
                    ImportExport.SetIdMap<User>(exportUser.Id, Import(portalId, exportUser, idMap), idMap);
            }
        }

        private string Import(string portalId, Models.User user, Dictionary<string, string> idMap, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Account.GetUser(user.Name, portalId);
            user.PortalId = portalId;
            user.Id = existing != null ? existing.Id : null;
            user.RoleIds = Security.GetNewRoleIds(user.RoleIds, idMap);
            return Account.SaveUser(user, userId);
        }

    }
}
