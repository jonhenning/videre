using System.Collections.Generic;
using CodeEndeavors.Extensions;
using Videre.Core.ContentProviders;

namespace Videre.Core.ContentProviders
{
    public class MenuContentProvider : IWidgetContentProvider 
    {
        T IWidgetContentProvider.Get<T>(List<string> ids) 
        {
            var id = ids.Count > 0 ? ids[0] : "";
            return Services.Menu.GetById(id) as T; //hack:  [0]?
        }

        public string GetJson(List<string> ids)
        {
            var id = ids.Count > 0 ? ids[0] : "";
            return Services.Menu.GetById(id).ToJson(); //todo:  pass in ignoreType? //hack:  [0]?
        }

        public Dictionary<string, string> Import(string portalId, string json, Dictionary<string, string> idMap)
        {
            var ret = new Dictionary<string, string>();
            if (json != null)
            {
                var menu = json.ToObject<Models.Menu>();
                //menu.Name = Namespace;
                menu.PortalId = portalId;
                ret[menu.Id] = Services.Menu.Import(portalId, menu, idMap);
            }
            return ret;
        }

        public List<string> Save(string json)
        {
            var ret = new List<string>();
            if (json != null)
            {
                var menu = json.ToObject<Models.Menu>();
                //menu.Name = Namespace;
                menu.PortalId = Services.Portal.CurrentPortalId; //todo: little hacky!
                ret.Add(Services.Menu.Save(menu, Services.Account.CurrentIdentityName));
            }
            return ret;
        }

        public void Delete(List<string> ids)
        {
            var id = ids.Count > 0 ? ids[0] : "";
            var menu = Services.Menu.GetById(id);
            if (menu != null)
                Services.Menu.Delete(menu.Id, Services.Account.CurrentIdentityName);
        }
    }

}
