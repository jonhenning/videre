using System.Collections.Generic;
using CodeEndeavors.Extensions.Serialization;

namespace Videre.Core.Models
{
    public class LayoutTemplate 
    {
        public LayoutTemplate()
        {
            Widgets = new List<Widget>();
            Roles = new List<string>();
            WebReferences = new List<string>();
        }

        public string Id { get; set; }
        public string LayoutName { get; set; }
        public string ThemeName { get; set; }
        public List<Widget> Widgets { get; set; }
        public List<string> Roles { get; set; }
        public string PortalId { get; set; }
        public List<string> WebReferences { get; set; }

        [SerializeIgnore(new string[] { "db", "client" })]         
        public Models.Theme Theme
        {
            get
            {
                return Services.UI.GetTheme(ThemeName);
            }
        }

        public Dictionary<string, string> GetWidgetContent()
        {
            var dict = new Dictionary<string, string>();
            string json;
            foreach (var widget in Widgets)
            {
                json = widget.GetContentJson();
                if (!string.IsNullOrEmpty(json))
                    dict[widget.Id] = json;
            }
            return dict;
        }

        [SerializeIgnore(new string[] {"db", "client"})] 
        public bool IsAuthorized { get { return Services.Account.RoleAuthorized(Roles); } }

    }

}
