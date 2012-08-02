using System.Collections.Generic;
using CodeEndeavors.Extensions;

namespace Videre.Core.ContentProviders
{
    public class LocalizationContentProvider : IWidgetContentProvider 
    {
        T IWidgetContentProvider.Get<T>(List<string> ids)
        {
            return Services.Localization.Get(ids) as T;
        }

        public string GetJson(List<string> ids)
        {
            return Services.Localization.Get(ids).ToJson(); 
        }

        public List<string> Save(string json)
        {
            var ret = new List<string>();
            if (json != null)
            {
                var locs = json.ToObject<List<Models.Localization>>();
                foreach (var loc in locs)
                {
                    loc.Type = Models.LocalizationType.WidgetContent;
                    //loc.Namespace = Namespace;
                    ret.Add(Services.Localization.Save(loc));
                }
            }
            return ret;
        }

        public Dictionary<string, string> Import(string portalId, string json, Dictionary<string, string> idMap)
        {
            var ret = new Dictionary<string, string>();
            if (json != null)
            {
                var locs = json.ToObject<List<Models.Localization>>();
                foreach (var loc in locs)
                {
                    loc.Type = Models.LocalizationType.WidgetContent;
                    //loc.Namespace = Namespace;
                    loc.PortalId = portalId;
                    ret[loc.Id] = Services.Localization.Import(portalId, loc);
                }
            }
            return ret;
        }

        public void Delete(List<string> ids)
        {
            var locs = Services.Localization.Get(ids); //Services.Localization.Get(Models.LocalizationType.WidgetContent, Services.Portal.CurrentPortalId, Namespace);
            foreach (var loc in locs)
            {
                Services.Localization.Delete(loc.Id, Services.Account.CurrentIdentityName);
            }
        }
    }

}
