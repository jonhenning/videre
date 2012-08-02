using System;
using System.Collections.Generic;
//using System.Web.Script.Serialization;
using CodeEndeavors.Extensions;
using CodeEndeavors.Extensions.Serialization;
//using Newtonsoft.Json;

namespace Videre.Core.Models 
{
    public class Widget : IClientControl
    {
        private string _id = null;
        //private string _clientId = null;
        private ContentProviders.IWidgetContentProvider _contentProvider = null;

        public Widget()
        {
            Attributes = new Dictionary<string, object>();
            ContentIds = new List<string>();
            Roles = new List<string>();
        }

        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                    _id = Guid.NewGuid().ToString();
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        //[ScriptIgnore, JsonIgnore]
        [SerializeIgnore(new string[] { "db", "client" })]
        public string ClientId { get; set; }

        //[ScriptIgnore, JsonIgnore]
        [SerializeIgnore(new string[] { "db", "client" })]
        public string Path
        {
            get
            {
                return Manifest.Path;
            }
        }
        //[ScriptIgnore, JsonIgnore]
        [SerializeIgnore(new string[] { "db", "client" })]
        public string ScriptPath
        {
            get
            {
                return string.Format("~/scripts/Widgets/{0}/", Path);
            }
        }

        [SerializeIgnore(new string[] { "db", "client" })]
        public string ContentPath
        {
            get
            {
                return string.Format("~/content/Widgets/{0}/", Path);
            }
        }


        public List<string> ContentIds { get; set; }
        public string ManifestId { get; set; }
        public string Css { get; set; }
        public string Style { get; set; }
        public string PaneName { get; set; }
        public int Seq { get; set; }
        public List<string> Roles { get; set; }
        public bool? Authenticated { get; set; }

        //[JsonIgnore]
        [SerializeIgnore("db")]
        public bool IsAuthorized 
        { 
            get 
            {
                return Services.Account.RoleAuthorized(Roles) &&
                    (!Authenticated.HasValue || Authenticated == Services.Account.IsAuthenticated);
            }
        }

        public Dictionary<string, object> Attributes { get; set; }

        //only used on client side for editors.  Way to pass content up associated to a widget when no Id exists
        //[JsonIgnore]
        [SerializeIgnore("db")]
        public string ContentJson { get; set; }

        public ContentProviders.IWidgetContentProvider GetContentProvider()
        {
            if (_contentProvider == null && !string.IsNullOrEmpty(Manifest.ContentProvider))
                _contentProvider = Manifest.ContentProvider.GetInstance<ContentProviders.IWidgetContentProvider>();
            return _contentProvider ;
        }

        public string GetContentJson()
        {
            if (!string.IsNullOrEmpty(Manifest.ContentProvider))
                return GetContentProvider().GetJson(ContentIds);
            return null;
        }

        public void SaveContentJson(string json)
        {
            if (!string.IsNullOrEmpty(Manifest.ContentProvider))   
            {
                if (json != null && json != "null") //todo: kinda hacky looking for string "null"
                    ContentIds = GetContentProvider().Save(json);
                else
                    ContentIds = new List<string>();
            }
        }

        public void RemoveContent()
        {
            var provider = GetContentProvider();
            if (provider != null && ContentIds.Count > 0)
                provider.Delete(ContentIds);
        }

        public T GetContent<T>() where T : class, new()
        {
            if (!string.IsNullOrEmpty(Manifest.ContentProvider))
                return GetContentProvider().Get<T>(ContentIds);
            return new T();
        }

        public string GetId(string id)
        {
            return string.Format("{0}_{1}", this.ClientId, id);
        }

        //public string GetResourcePath(string templateId)
        //{
        //    return templateId + "/" + Id; 
        //}

        //[ScriptIgnore, JsonIgnore]
        [SerializeIgnore(new string[] { "db", "client" })]
        public Models.WidgetManifest Manifest
        {
            get
            {
                return Services.Portal.GetWidgetManifestById(ManifestId);
            }
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
            return Services.Localization.GetLocalization(LocalizationType.Widget, key, defaultValue, Manifest.FullName);
        }

        public string GetPortalText(string key, string defaultValue)
        {
            return Services.Localization.GetPortalText(key, defaultValue);
        }

    }

}
