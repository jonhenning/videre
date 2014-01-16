using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//todo: change namespace to just Providers
namespace Videre.Core.ContentProviders
{
    public interface IWidgetContentProvider
    {
        string GetJson(List<string> ids, string ignoreType = null);
        T Get<T>(List<string> ids) where T : class;
        List<string> Save(string ns, string json);
        Dictionary<string, string> Import(string portalId, string ns, string json, Dictionary<string, string> idMap); //return <oldId, newId>
        void Delete(List<string> ids);
    }
}
