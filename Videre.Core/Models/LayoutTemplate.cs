using System.Collections.Generic;
using CodeEndeavors.Extensions;
using CodeEndeavors.Extensions.Serialization;
using Videre.Core.Services;

namespace Videre.Core.Models
{
    public class LayoutTemplate : IAuthorizationEntity
    {
        public LayoutTemplate()
        {
            Attributes = new Dictionary<string, object>();
            Widgets = new List<Widget>();
            //RoleIds = new List<string>();
            ExcludeRoleIds = new List<string>();
            Claims = new List<UserClaim>();
            WebReferences = new List<string>();
        }
        private string _layoutViewName = null;

        public string Id { get; set; }
        public string LayoutName { get; set; }
        public string LayoutViewName
        {
            get
            {
                return string.IsNullOrEmpty(_layoutViewName) ? LayoutName : _layoutViewName;
            }
            set
            {
                _layoutViewName = value;
            }
        }
        public string ThemeName { get; set; }
        public List<Widget> Widgets { get; set; }

        public bool? Authenticated { get; set; }
        public List<UserClaim> Claims { get; set; }
        //public List<string> RoleIds { get; set; }
        public List<string> ExcludeRoleIds { get; set; }

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
        public bool IsAuthorized { get { return Authorization.IsAuthorized(this); } }

        public Dictionary<string, object> Attributes { get; set; }

        public T GetAttribute<T>(string key, T defaultValue)
        {
            try
            {
                return Attributes.GetSetting(key, defaultValue);
            }
            catch
            {
            }
            return defaultValue;
        }

        public bool Merge(LayoutTemplate from)
        {
            var updated = false;
            if (this.LayoutViewName != from.LayoutViewName)
            {
                this.LayoutViewName = from.LayoutViewName;
                updated = true;
            }
            if (this.ThemeName != from.ThemeName)
            {
                this.ThemeName = from.ThemeName;
                updated = true;
            }

            //todo: should we allow merging of roles, claims and authenticated?
            if (this.Authenticated != from.Authenticated)
            {
                this.Authenticated = from.Authenticated;
                updated = true;
            }

            foreach (var widget in from.Widgets)
            {
                if (!this.Widgets.Exists(w => w.PaneName == widget.PaneName && w.ManifestId == widget.ManifestId))
                {
                    this.Widgets.Add(widget);
                    updated = true;
                }
            }
            return updated;
        }


    }

}
