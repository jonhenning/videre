using System.Collections.Generic;
using System.Web.Mvc;

namespace Videre.Core.Models
{
    public interface IClientControl
    {
        string ClientId { get; }

        string Path { get; }

        string ScriptPath { get; }

        string GetId(string id);

        string GetText(string key, string defaultValue);

        bool Register(HtmlHelper helper, string clientType, string instanceName, Dictionary<string, object> properties = null, bool preserveObjectReferences = false);

        string GetPortalText(string key, string efaultValue);
    }
}