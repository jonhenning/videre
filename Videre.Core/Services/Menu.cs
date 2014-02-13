using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainObjects = CodeEndeavors.ResourceManager.DomainObjects;
using Videre.Core.Models;
using CodeEndeavors.Extensions;

namespace Videre.Core.Services
{
    public class Menu
    {
        public static Models.Menu GetById(string id)
        {
            var res = Repository.Current.GetResourceById<Models.Menu>(id);
            if (res != null)
                return res.Data;
            return null;
        }
        
        public static List<Models.Menu> Get(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return Repository.Current.GetResources<Models.Menu>("Menu", m => m.Data.PortalId == portalId, false).Select(f => f.Data).ToList();
        }

        public static Models.Menu Get(string portalId, string name)
        {
            return Repository.Current.GetResourceData<Models.Menu>("Menu", m => m.Data.PortalId == portalId && m.Data.Name == name, null);
        }

        public static string Import(string portalId, Models.Menu menu, Dictionary<string, string> idMap, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Get(portalId, menu.Name);
            if (existing != null)
                menu.Id = existing.Id;
            else
                menu.Id = null;
            menu.PortalId = portalId;
            ImportSubMenuItems(menu.Items, idMap);

            return Save(menu, userId);
        }

        private static void ImportSubMenuItems(List<Models.MenuItem> items, Dictionary<string, string> idMap)
        {
            foreach (var subItem in items)
            {
                subItem.RoleIds = Security.GetNewRoleIds(subItem.RoleIds, idMap);
                ImportSubMenuItems(subItem.Items, idMap);
            }
        }

        public static string Save(Models.Menu menu, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            menu.PortalId = string.IsNullOrEmpty(menu.PortalId) ? Services.Portal.CurrentPortalId : menu.PortalId;
            Validate(menu);
            var res = Repository.Current.StoreResource("Menu", null, menu, userId);
            return res.Id;
        }

        public static void Validate(Models.Menu menu)
        {
            if (string.IsNullOrEmpty(menu.Name))
                throw new Exception(Localization.GetExceptionText("InvalidResource.Error", "{0} is invalid.", "Menu"));
            if (IsDuplicate(menu))
                throw new Exception(Localization.GetExceptionText("DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "Menu"));
        }

        public static bool IsDuplicate(Models.Menu menu)
        {
            var r = Get(menu.PortalId, menu.Name);
            if (r != null)
                return r.Id != menu.Id;
            return false;
        }

        public static bool Exists(Models.Menu menu)
        {
            var existingMenu = Get(menu.PortalId, menu.Name); 
            return existingMenu != null && existingMenu.Name == menu.Name;
        }

        public static bool Delete(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var menu = Repository.Current.GetResourceById<Models.Menu>(id);
            if (menu != null)
                Repository.Current.Delete(menu);
            return menu != null;
        }

    }
}
