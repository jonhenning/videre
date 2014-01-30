using System.Collections.Generic;
using System.Web.Mvc;
using CodeEndeavors.Extensions;
using CodeEndeavors.Extensions.Serialization;
using Videre.Core.Extensions;

namespace Videre.Core.Models
{
    //allows you to create a widget with multiple sub widgets and still maintain the ability to namespace correctly
    //most of the logic just defers to the passed in widget, including the retrieval of text and script paths.
    public class SubWidget : IClientControl
    {
        private readonly string _subClientId;

        public SubWidget(Widget widget) : this(widget, true)
        {
        }

        public SubWidget(Widget widget, bool separateNamespace)
        {
            Attributes = new Dictionary<string, object>();
            Widget = widget;
            _subClientId = separateNamespace ? Services.Portal.NextClientId() : "";
        }

        public Widget Widget { get; set; }

        [SerializeIgnore(new[] {"db", "client"})]
        public string ContentPath
        {
            get { return Widget.ContentPath; }
        }

        public Dictionary<string, object> Attributes { get; set; }

        [SerializeIgnore(new[] {"db", "client"})]
        public string ClientId
        {
            get 
            { 
                if (!string.IsNullOrEmpty(_subClientId))
                    return string.Format("{0}_{1}", _subClientId, Widget.ClientId);
                return Widget.ClientId;
            }
        }

        [SerializeIgnore(new[] {"db", "client"})]
        public string Path
        {
            get { return Widget.Path; }
        }

        [SerializeIgnore(new[] {"db", "client"})]
        public string ScriptPath
        {
            get { return Widget.ScriptPath; }
        }

        public string GetId(string id)
        {
            return string.Format("{0}_{1}", ClientId, id);
        }

        public string GetText(string key, string defaultValue)
        {
            return Widget.GetText(key, defaultValue);
        }

        public string GetPortalText(string key, string efaultValue)
        {
            return Widget.GetPortalText(key, efaultValue);
        }

        public bool Register(HtmlHelper helper, string clientType, string instanceName, Dictionary<string, object> properties = null, bool preserveObjectReferences = false)
        {
            properties = properties ?? new Dictionary<string, object>();

            helper.RegisterCoreScripts();

            if (!string.IsNullOrEmpty(ScriptPath))
                helper.RegisterScript(ScriptPath + clientType + ".js");

            properties["id"] = ClientId;
            properties["ns"] = ClientId;
            properties["wid"] = Widget.Id;
            //properties["user"] = Services.Account.GetClientUser();

            helper.RegisterDocumentReadyScript(
                ClientId + "Presenter",
                string.Format("videre.widgets.register('{0}', {1}, {2});", ClientId, clientType, properties.ToJson(false, "client", preserveObjectReferences)));

            return true;
        }

        public T GetAttribute<T>(string key, T defaultValue)
        {
            //TODO: BIG HACK... IF CANNOT CAST WE SHOULD NOT TRAP WITH EXCEPTION
            try
            {
                return Attributes.GetSetting(key, defaultValue);
            }
            catch
            {
            }
            return defaultValue;
        }
    }
}