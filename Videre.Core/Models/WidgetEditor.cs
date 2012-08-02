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
            this.ManifestId = manifestId;
        }

        public string ManifestId { get; set; }

        //[ScriptIgnore, JsonIgnore]
        [SerializeIgnore(new string[] { "db", "client" })]
        public string ClientId { get; set; }    //must be assigned every time the widget is rendered

        public Models.WidgetManifest Manifest
        {
            get
            {
                return Services.Portal.GetWidgetManifestById(ManifestId);
            }
        }

        //[ScriptIgnore, JsonIgnore]
        [SerializeIgnore(new string[] { "db", "client" })]
        public string Path
        {
            get
            {
                return string.IsNullOrEmpty(Manifest.EditorPath) ? "Widgets/Core/CommonEditor" : Manifest.EditorPath;
            }
        }

        //[ScriptIgnore, JsonIgnore]
        [SerializeIgnore(new string[] { "db", "client" })]
        public string ScriptPath
        {
            get
            {
                return string.Format("~/scripts/Widgets/../{0}/../", Path); //todo: royal hack.  really bad jon.  need to fix!
            }
        }

        public string GetId(string Id)
        {
            return string.Format("{0}_{1}", this.ClientId, Id);
        }

        public string GetText(string key, string defaultValue)
        {
            return Services.Localization.GetLocalization(LocalizationType.WidgetEditor, key, defaultValue, Manifest.EditorPath);
        }

        public string GetPortalText(string key, string defaultValue)
        {
            return Services.Localization.GetPortalText(key, defaultValue);
        }


    }

}
