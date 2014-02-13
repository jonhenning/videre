using System.Collections.Generic;
using CodeEndeavors.Extensions.Serialization;
//using Newtonsoft.Json;

namespace Videre.Core.Models 
{
    public class Menu 
    {
        public enum MenuType
        {
            DropDown//,
            //NavList
        }

        public Menu()
        {
            Items = new List<MenuItem>();
        }

        public string Id { get; set; }
        public string PortalId { get; set; }
        public string Text { get; set; }
        public string TextCss { get; set; }
        public string Name { get; set; }
        //public string Css { get; set; }
        //public bool ShowSearch { get; set; }
        //public bool InverseColors { get; set; }
        public MenuType Type { get; set; }
        public List<MenuItem> Items { get; set; }
    }

    public class MenuItem
    {
        public MenuItem()
        {
            Items = new List<MenuItem>();
            //Roles = new List<string>();
        }

        public string Text { get; set; }    //TODO: LOCALIZE!
        public string Icon { get; set; }
        public List<MenuItem> Items { get; set; }
        public string TemplateId { get; set; }  //??? - think its for eventual mapping to template, regardless of its url...
        public string Url { get; set; }
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
        public bool? Authenticated { get; set; }
        public bool AlignRight { get; set; }

        //[JsonIgnore]
        [SerializeIgnore("db")] //todo: we need this on client?  ignore "client"
        public bool IsAuthorized
        {
            get
            {
                if (RoleIds == null || RoleIds.Count == 0 || RoleIds.Exists(r => Services.Account.IsInRole(r)))       //todo: how can we get null roles?
                {
                    if (!Authenticated.HasValue || Authenticated.Value == Services.Account.IsAuthenticated) 
                        return true;
                }
                return false;
            }
        }

    }

}
