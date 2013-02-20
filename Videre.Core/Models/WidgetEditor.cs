using System.Collections.Generic;
using System.Web.Mvc;
using CodeEndeavors.Extensions.Serialization;

namespace Videre.Core.Models
{
    public class WidgetEditor : IClientControl
    {
        public WidgetEditor()
        {
        }

        public WidgetEditor(string manifestId)
        {
            ManifestId = manifestId;
        }

        public string ManifestId { get; set; }

        //must be assigned every time the widget is rendered

        public WidgetManifest Manifest
        {
            get { return Services.Portal.GetWidgetManifestById(ManifestId); }
        }

        [SerializeIgnore(new[] {"db", "client"})]
        public string ClientId { get; set; }

        [SerializeIgnore(new[] {"db", "client"})]
        public string Path
        {
            get { return string.IsNullOrEmpty(Manifest.EditorPath) ? "Widgets/Core/CommonEditor" : Manifest.EditorPath; }
        }

        //todo: royal hack.  really bad jon.  need to fix!
        [SerializeIgnore(new[] {"db", "client"})]
        public string ScriptPath
        {
            get { return string.Format("~/scripts/Widgets/../{0}/../", Path); }
        }

        public string GetId(string id)
        {
            return string.Format("{0}_{1}", ClientId, id);
        }

        public string GetText(string key, string defaultValue)
        {
            return Services.Localization.GetLocalization(LocalizationType.WidgetEditor, key, defaultValue, Manifest.EditorPath);
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