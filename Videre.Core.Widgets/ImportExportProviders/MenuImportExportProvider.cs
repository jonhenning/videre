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
    public class MenuImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Menu"; } }
        public List<string> ProviderDependencies
        {
            get { return new List<string>() { "Role" }; }
        }
        
        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Menu.Get(portalId).Select(m =>
                new ImportExportContent()
                {
                    Id = m.Id,
                    Name = m.Name,
                    Type = Name,
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

        public void Import(PortalExport export, Dictionary<string, string> idMap, string portalId)
        {
            if (export.Menus != null)
            {
                Logging.Logger.DebugFormat("Importing {0} menus...", export.Menus.Count);
                foreach (var menu in export.Menus)
                    ImportExport.SetIdMap<Models.Menu>(menu.Id, Services.Menu.Import(portalId, menu, idMap), idMap);
            }
        }

        //since currently part of content provider, place in service 
        //private string Import(string portalId, Models.Menu menu, Dictionary<string, string> idMap, string userId = null)
        //{
        //    userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
        //    var existing = Services.Menu.Get(portalId, menu.Name);
        //    if (existing != null)
        //        menu.Id = existing.Id;
        //    else
        //        menu.Id = null;
        //    menu.PortalId = portalId;
        //    ImportSubMenuItems(menu.Items, idMap);

        //    return Services.Menu.Save(menu, userId);
        //}

        //private void ImportSubMenuItems(List<Models.MenuItem> items, Dictionary<string, string> idMap)
        //{
        //    foreach (var subItem in items)
        //    {
        //        subItem.RoleIds = Security.GetNewRoleIds(subItem.RoleIds, idMap);
        //        ImportSubMenuItems(subItem.Items, idMap);
        //    }
        //}

    }
}
