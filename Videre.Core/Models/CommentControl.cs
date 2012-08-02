
namespace Videre.Core.Models
{
    public class CommentControl : IClientControl
    {
        public CommentControl(string containerType, string containerId, string portalId = null)
        {
            ClientId = Services.Portal.NextClientId();

            Container = Services.Comment.Get(containerType, containerId, portalId);
            if (Container == null)
            {
                Container = new CommentContainer() { ContainerType = containerType, ContainerId = containerId };
                Services.Comment.Save(Container);
            }
        }

        public CommentContainer Container {get;set;}
        public string ClientId { get; set; }    //must be assigned every time the widget is rendered

        public string Path
        {
            get
            {
                return "Controls/Core/Comments";
            }
        }

        public string ScriptPath
        {
            get
            {
                return "~/scripts/Controls/Core/Comments/";
            }
        }

        public string GetId(string Id)
        {
            return string.Format("{0}_{1}", this.ClientId, Id);
        }

        public string GetText(string key, string defaultValue)
        {
            return Services.Localization.GetLocalization(LocalizationType.ClientControl, key, defaultValue, this.Path);
        }

        public string GetPortalText(string key, string defaultValue)
        {
            return Services.Localization.GetPortalText(key, defaultValue);
        }

    }

}
