using System.Collections.Generic;
using CodeEndeavors.Extensions;
using Videre.Core.ContentProviders;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Widgets.ContentProviders
{
    public class MenuContentProvider : IWidgetContentProvider 
    {
        T IWidgetContentProvider.Get<T>(List<string> ids) 
        {
            var id = ids.Count > 0 ? ids[0] : "";
            return CoreServices.Menu.GetById(id) as T; //hack:  [0]?
        }

        public string GetJson(List<string> ids, string ignoreType = null)
        {
            var id = ids.Count > 0 ? ids[0] : "";
            return CoreServices.Menu.GetById(id).ToJson(ignoreType: ignoreType); //hack:  [0]?
        }

        public Dictionary<string, string> Import(string portalId, string ns, string json, Dictionary<string, string> idMap)
        {
            var ret = new Dictionary<string, string>();
            if (json != null)
            {
                var menu = json.ToObject<CoreModels.Menu>();
                //menu.Name = Namespace;
                menu.PortalId = portalId;
                ret[menu.Id] = CoreServices.Menu.Import(portalId, menu, idMap);
            }
            return ret;
        }

        public List<string> Save(string ns, string json)
        {
            var ret = new List<string>();
            if (json != null)
            {
                var menu = json.ToObject<CoreModels.Menu>();
                //menu.Name = Namespace;
                menu.PortalId = CoreServices.Portal.CurrentPortalId; //todo: little hacky!
                ret.Add(CoreServices.Menu.Save(menu));
            }
            return ret;
        }

        public void Delete(List<string> ids)
        {
            var id = ids.Count > 0 ? ids[0] : "";
            var menu = CoreServices.Menu.GetById(id);
            if (menu != null)
                CoreServices.Menu.Delete(menu.Id);
        }
    }

}
