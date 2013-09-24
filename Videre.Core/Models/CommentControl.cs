using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Videre.Core.Models
{
    public class CommentControl : IClientControl
    {
        public CommentControl(Widget widget, string containerType, string containerId, string portalId = null)
        {
            ClientId = Services.Portal.NextClientId();
            Widget = widget;
            Provider = widget.GetAttribute("CommentProvider", "None");

            if (Provider.Equals("Videre", StringComparison.InvariantCultureIgnoreCase))
            {
                Container = Services.Comment.Get(containerType, containerId, portalId);
                if (Container == null)
                {
                    Container = new CommentContainer {ContainerType = containerType, ContainerId = containerId};
                    Services.Comment.Save(Container);
                }
            }
        }

        public CommentContainer Container { get; set; }

        //must be assigned every time the widget is rendered
        public Widget Widget { get; set; }

        public string Provider { get; set; }

        public string ClientId { get; set; }

        public string Path
        {
            get { return "Controls/Core/Comments"; }
        }

        public string ScriptPath
        {
            get { return "~/scripts/Controls/Core/Comments/"; }
        }

        public string GetId(string id)
        {
            return string.Format("{0}_{1}", ClientId, id);
        }

        public string GetText(string key, string defaultValue)
        {
            return Services.Localization.GetLocalization(LocalizationType.ClientControl, key, defaultValue, Path);
        }

        public bool Register(HtmlHelper helper, string clientType, string instanceName, Dictionary<string, object> properties = null, bool preserveObjectReferences = false)
        {
            return false;
        }

        public string GetPortalText(string key, string efaultValue)
        {
            return Services.Localization.GetPortalText(key, efaultValue);
        }
    }
}