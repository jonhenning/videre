using System;

namespace Videre.Core.Models 
{
    public class View : IClientControl
    {
        private string _id = null;

        public View()
        {
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

        public string ClientId { get; set; }

        public string Path
        {
            get
            {
                return "";
            }
        }

        public string ScriptPath
        {
            get
            {
                return string.Format("~/scripts/Widgets/{0}/", Path);
            }
        }

        public string GetId(string id)
        {
            return string.Format("{0}_{1}", this.ClientId, id);
        }

        public string GetText(string key, string defaultValue)
        {
            return defaultValue;
            //throw new NotImplementedException("A View does not have specific text associated to it.  Use GetPortalText instead");
        }

        public string GetPortalText(string key, string defaultValue)
        {
            return Services.Localization.GetPortalText(key, defaultValue);
        }

    }

}
