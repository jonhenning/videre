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
    public class SecureActivityImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Secure Activity"; } }
        public List<string> ProviderDependencies
        {
            get { return new List<string>() { "Role" }; }
        }

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
                export.Roles.AddRange(Services.Account.GetRoles(portalId).Where(r => a.RoleIds.Contains(r.Id)));
            }

            return export;
        }

        public void Import(PortalExport export, Dictionary<string, string> idMap, string portalId)
        {
            if (export.SecureActivities != null)
            {
                Logging.Logger.DebugFormat("Importing {0} secure activities...", export.SecureActivities.Count);
                foreach (var exportActivity in export.SecureActivities)
                    ImportExport.SetIdMap<SecureActivity>(exportActivity.Id, Import(portalId, exportActivity, idMap), idMap);
            }
        }

        private string Import(string portalId, Models.SecureActivity activity, Dictionary<string, string> idMap, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Services.Security.GetSecureActivity(portalId, activity.Area, activity.Name);
            activity.Id = existing != null ? existing.Id : null;
            activity.PortalId = portalId;
            activity.RoleIds = Services.Security.GetNewRoleIds(activity.RoleIds, idMap);
            return Services.Security.Save(activity, userId);
        }

    }
}
