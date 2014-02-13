using System.Collections.Generic;
using CodeEndeavors.Extensions.Serialization;

namespace Videre.Core.Models
{
    public class LayoutTemplate 
    {
        public LayoutTemplate()
        {
            Widgets = new List<Widget>();
            //Roles = new List<string>();
            WebReferences = new List<string>();
        }

        public string Id { get; set; }
        public string LayoutName { get; set; }
        public string ThemeName { get; set; }
        public List<Widget> Widgets { get; set; }
        private List<string> _roles = new List<string>();
        [System.Obsolete("Use RoleIds")]
        [SerializeIgnore(new string[] { "client" })]
        public List<string> Roles
        {
            get
            {
                return _roles;
            }
            set
            {
                _roles = value;
            }
        }

        private List<string> _roleIds = new List<string>();
        public List<string> RoleIds
        {
            get
            {
                if (_roles != null && _roles.Count > 0)
                {
                    _roleIds.AddRange(_roles);
                    _roles.Clear();
                }
                return _roleIds;
            }
            set
            {
                _roleIds = value;
            }
        }
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
        public bool IsAuthorized { get { return Services.Account.RoleAuthorized(RoleIds); } }

    }

}
