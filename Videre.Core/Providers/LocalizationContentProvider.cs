using System.Collections.Generic;
using CodeEndeavors.Extensions;

//todo: change namespace to just Providers
namespace Videre.Core.ContentProviders
{
    public class LocalizationContentProvider : IWidgetContentProvider 
    {
        //todo:  always tokenize???
        T IWidgetContentProvider.Get<T>(List<string> ids)
        {
            return Services.Localization.Get(ids) as T;
        }

        public string GetJson(List<string> ids, string ignoreType = null)
        {
            return Services.Localization.Get(ids).ToJson(ignoreType: ignoreType);
        }

        public List<string> Save(string ns, string json)
        {
            var ret = new List<string>();
            if (json != null)
            {
                var locs = json.ToObject<List<Models.Localization>>();
                foreach (var loc in locs)
                {
                    loc.Type = Models.LocalizationType.WidgetContent;
                    loc.PortalId = string.IsNullOrEmpty(loc.PortalId) ? Services.Portal.CurrentPortalId : loc.PortalId;
                    
                    //loc.Text = Services.TokenParser.ReplaceContentWithTokens(loc.Text);

                    var singleInstanceNamespace = "__" + ns;//using __ to allow distinction between shared content vs. single instance - minor hack!
                    if (string.IsNullOrEmpty(loc.Namespace))
                        loc.Namespace = singleInstanceNamespace;

                    if (loc.Namespace != singleInstanceNamespace) //if we are sharing a namespace check for singleinstance existance and remove if found
                    {
                        var existing = Services.Localization.Get(loc.PortalId, loc.Type, singleInstanceNamespace, loc.Key, loc.Locale);
                        if (existing != null)   //if single use exists, clean it up!
                            Services.Localization.Delete(existing.Id);
                    }
                    ret.Add(Services.Localization.Save(loc));
                }
            }
            return ret;
        }

        public Dictionary<string, string> Import(string portalId, string ns, string json, Dictionary<string, string> idMap)
        {
            var ret = new Dictionary<string, string>();
            if (json != null)
            {
                var locs = json.ToObject<List<Models.Localization>>();
                foreach (var loc in locs)
                {
                    loc.Type = Models.LocalizationType.WidgetContent;
                    //loc.Text = Services.TokenParser.ReplaceContentWithTokens(loc.Text);   //import shouldn't need this as already tokenized

                    if (string.IsNullOrEmpty(loc.Namespace))    //todo: not necessary, as all exports should contain namespace!
                        loc.Namespace = "__" + ns;
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
