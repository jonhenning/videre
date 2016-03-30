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
            var res = Repository.GetResourceById<Models.Menu>(id);
            if (res != null)
                return res.Data;
            return null;
        }
        
        public static List<Models.Menu> Get(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return Repository.GetResources<Models.Menu>("Menu", m => m.Data.PortalId == portalId, false).Select(f => f.Data).ToList();
        }

        public static Models.Menu Get(string portalId, string name)
        {
            return Repository.GetResourceData<Models.Menu>("Menu", m => m.Data.PortalId == portalId && m.Data.Name == name, null);
        }

        //since currently part of content provider, place in service 
        public static string Import(string portalId, Models.Menu menu, Dictionary<string, string> idMap, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Get(portalId, menu.Name);
            if (existing != null)
                menu.Id = existing.Id;
            else
                menu.Id = null;
            menu.PortalId = portalId;
            ImportSubMenuItems(menu.Items, idMap, existing != null ? existing.Items : new List<MenuItem>());

            return Save(menu, userId);
        }

        private static void ImportSubMenuItems(List<Models.MenuItem> items, Dictionary<string, string> idMap, List<Models.MenuItem> existingItems)
        {
            foreach (var subItem in items)
            {
                var existingItem = existingItems.Where(i => i.Text == subItem.Text).FirstOrDefault();
                subItem.RoleIds = Security.GetNewRoleIds(subItem.RoleIds, idMap);
                ImportSubMenuItems(subItem.Items, idMap, existingItem != null ? existingItem.Items : new List<MenuItem>());
            }

            //allow existing items to stay...  no need for recursion here as we get it all by adding its parent.
            foreach (var existingItem in existingItems)
            {
                if (!items.Exists(i => i.Text == existingItem.Text))
                    items.Add(existingItem);
            }
        }

        public static int AddMenuItem(string menuName, Models.MenuItem newItem, int? position = null)
        {
            var menu = Menu.Get(Portal.CurrentPortalId, menuName);
            if (menu == null)
            {
                menu = new Models.Menu() { Name = menuName, Items = new List<Models.MenuItem>() { newItem } };
                Menu.Save(menu);
                return 1;
            }
            else
                return AddMenuItem(menu, menu.Items, newItem, position);
        }
        public static int AddMenuItem(Models.Menu menu, List<Models.MenuItem> parentItems, Models.MenuItem newItem, int? position = null)
        {
            var item = parentItems.Where(i => i.Text.Equals(newItem.Text, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (item == null)
            {
                if (position.HasValue && parentItems.Count > position)
                    parentItems.Insert(position.Value, newItem);
                else
                    parentItems.Add(newItem);
                Menu.Save(menu);
                return 1;
            }
            else   //item existed, so make sure all children also exist
            {
                var updates = 0;

                //allow the following overrides
                if (item.Icon != newItem.Icon)
                {
                    item.Icon = newItem.Icon;
                    updates++;
                }
                if (item.Url != newItem.Url)
                {
                    item.Url = newItem.Url;
                    updates++;
                }
                if (updates > 0)
                    Menu.Save(menu);

                foreach (var childNewItem in newItem.Items)
                    updates += AddMenuItem(menu, item.Items, childNewItem);
                return updates;
            }
        }

        public static string RegisterMenu(string name, string text, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var menu = Get(Portal.CurrentPortalId, name);
            if (menu == null)
                menu = new Models.Menu()
                {
                    Name = name,
                    PortalId = Portal.CurrentPortalId
                };
            menu.Text = text;
            return Save(menu, userId);
        }

        public static string Save(Models.Menu menu, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            menu.PortalId = string.IsNullOrEmpty(menu.PortalId) ? Services.Portal.CurrentPortalId : menu.PortalId;
            Validate(menu);
            var res = Repository.StoreResource("Menu", null, menu, userId);
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
            var menu = Repository.GetResourceById<Models.Menu>(id);
            if (menu != null)
                Repository.Delete(menu);
            return menu != null;
        }

    }
}
