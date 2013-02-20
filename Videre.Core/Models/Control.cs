using System.Collections.Generic;
using System.Web.Mvc;

namespace Videre.Core.Models
{
    public class Control : IClientControl
    {
        private readonly string _path;

        public Control(string path)
        {
            _path = path;
        }

        public string Path
        {
            get { return _path; }
        }

        public string ClientId { get; set; } //must be assigned every time the widget is rendered

        public string ScriptPath
        {
            get { return string.Format("~/scripts/Controls/{0}/", Path); }
        }

        public string GetId(string id)
        {
            return string.Format("{0}_{1}", this.ClientId, id);
        }

        public string GetText(string key, string defaultValue)
        {
            return Services.Localization.GetLocalization(LocalizationType.ClientControl, key, defaultValue, this.Path);
        }

        public bool Register(HtmlHelper helper, string clientType, string instanceName, Dictionary<string, object> properties = null)
        {
            return false;
        }

        public string GetPortalText(string key, string efaultValue)
        {
            return Services.Localization.GetPortalText(key, efaultValue);
        }
    }
}