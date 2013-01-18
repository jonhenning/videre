using System;
using System.Linq;
using System.Collections.Generic;
//using System.Web.Script.Serialization;
using CodeEndeavors.Extensions;
using CodeEndeavors.Extensions.Serialization;
//using Newtonsoft.Json;

namespace Videre.Core.Models 
{

    //allows you to create a widget with multiple sub widgets and still maintain the ability to namespace correctly
    //most of the logic just defers to the passed in widget, including the retrieval of text and script paths.
    public class SubWidget : IClientControl
    {
        private string _subClientId;
        public Widget Widget { get; set; }

        public SubWidget(Widget widget)
        {
            Attributes = new Dictionary<string, object>();
            Widget = widget;
            _subClientId = Services.Portal.NextClientId();
        }

        //[ScriptIgnore, JsonIgnore]
        [SerializeIgnore(new string[] { "db", "client" })]
        public string ClientId 
        {
            get
            {
                return string.Format("{0}_{1}", _subClientId, Widget.ClientId);
            }
        }

        [SerializeIgnore(new string[] { "db", "client" })]
        public string Path
        {
            get
            {
                return Widget.Path;
            }
        }

        //[ScriptIgnore, JsonIgnore]
        [SerializeIgnore(new string[] { "db", "client" })]
        public string ScriptPath
        {
            get
            {
                return Widget.ScriptPath;
            }
        }

        [SerializeIgnore(new string[] { "db", "client" })]
        public string ContentPath
        {
            get
            {
                return Widget.ContentPath;
            }
        }

        public Dictionary<string, object> Attributes { get; set; }

        public string GetId(string id)
        {
            return string.Format("{0}_{1}", this.ClientId, id);
        }

        public T GetAttribute<T>(string key, T defaultValue)
        {
            //TODO: BIG HACK... IF CANNOT CAST WE SHOULD NOT TRAP WITH EXCEPTION
            try
            {
                return Attributes.GetSetting<T>(key, defaultValue);
            }
            catch { }
            return defaultValue;
        }

        public string GetText(string key, string defaultValue)
        {
            return Widget.GetText(key, defaultValue);
        }

        public string GetPortalText(string key, string defaultValue)
        {
            return Widget.GetPortalText(key, defaultValue);
        }
    }

}
