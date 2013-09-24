using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CodeEndeavors.Extensions;
using CodeEndeavors.Extensions.Serialization;
using Videre.Core.Extensions;
using Videre.Core.Services;

namespace Videre.Core.Models
{
    public class Widget : IClientControl
    {
        private string _id;

        public Widget()
        {
            Attributes = new Dictionary<string, object>();
            ContentIds = new List<string>();
            Roles = new List<string>();
            WebReferences = new List<string>();
        }

        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                    _id = Guid.NewGuid().ToString();
                return _id;
            }
            set { _id = value; }
        }

        [SerializeIgnore(new[] {"db", "client"})]
        public string ContentPath
        {
            get { return string.Format("~/content/Widgets/{0}/", Path); }
        }

        
        public List<string> ContentIds { get; set; }

        public string ManifestId { get; set; }

        public string Css { get; set; }

        public string Style { get; set; }

        public string PaneName { get; set; }
        
        public bool ShowHeader { get; set; }
        
        public int Seq { get; set; }

        public List<string> Roles { get; set; }

        public bool? Authenticated { get; set; }

        public List<string> WebReferences { get; set; }

        [SerializeIgnore("db")]
        public bool IsAuthorized
        {
            get
            {
                return Account.RoleAuthorized(Roles) &&
                    (!Authenticated.HasValue || Authenticated == Account.IsAuthenticated);
            }
        }

        public Dictionary<string, object> Attributes { get; set; }

        [SerializeIgnore("db")]
        public string ContentJson { get; set; }

        [SerializeIgnore(new[] {"db", "client"})]
        public WidgetManifest Manifest
        {
            get { return Services.Widget.GetWidgetManifestById(ManifestId); }
        }

        [SerializeIgnore(new[] {"db", "client"})]
        public string ClientId { get; set; }

        [SerializeIgnore(new[] {"db", "client"})]
        public string Path
        {
            get { return Manifest.Path; }
        }

        [SerializeIgnore(new[] {"db", "client"})]
        public string ScriptPath
        {
            get { return string.Format("~/scripts/Widgets/{0}/", Path); }
        }

        public string GetId(string id)
        {
            return string.Format("{0}_{1}", ClientId, id);
        }

        public string GetText(string key, string defaultValue)
        {
            return Services.Localization.GetLocalization(LocalizationType.Widget, key, defaultValue, Manifest.FullName);
        }

        public bool Register(HtmlHelper helper, string clientType, string instanceName, Dictionary<string, object> properties = null, bool preserveObjectReferences = false)
        {
            properties = properties ?? new Dictionary<string, object>();

            helper.RegisterCoreScripts();

            if (!string.IsNullOrEmpty(ScriptPath))
                helper.RegisterScript(ScriptPath + clientType + ".js");

            properties["id"] = ClientId;
            properties["ns"] = ClientId;
            properties["wid"] = Id;
            //properties["user"] = Services.Account.GetClientUser();

            helper.RegisterDocumentReadyScript(
                ClientId + "Presenter",
                string.Format("videre.widgets.register('{0}', {1}, {2});", ClientId, clientType, properties.ToJson(ignoreType: "client", preserveObjectReferences: preserveObjectReferences)));

            return true;
        }

        public string GetPortalText(string key, string efaultValue)
        {
            return Services.Localization.GetPortalText(key, efaultValue);
        }

        public string GetContentJson()
        {
            var provider = Manifest.GetContentProvider();
            return provider != null ? provider.GetJson(ContentIds) : null;
        }

        public void SaveContentJson(string json)
        {
            var provider = Manifest.GetContentProvider();
            if (provider == null) return;

            if (json != null && json != "null") //todo: kinda hacky looking for string "null"
                ContentIds = provider.Save(Id, json);
            else
                ContentIds = new List<string>();
        }

        public void RemoveContent()
        {
            var sharedContentIds = Services.Portal.GetSharedContentIds();
            if (ContentIds.Count <= 0) return;

            var contentProvider = Manifest.GetContentProvider();
            if (contentProvider == null) return;

            var nonSharedIds = ContentIds.Where(i => !sharedContentIds.Contains(i)).ToList();
            contentProvider.Delete(nonSharedIds);
        }

        public T GetContent<T>() where T : class, new()
        {
            var provider = Manifest.GetContentProvider();
            return provider != null ? provider.Get<T>(ContentIds) : new T();
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