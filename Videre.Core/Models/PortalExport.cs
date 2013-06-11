using CodeEndeavors.Extensions.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.Models
{
    public class PortalExport
    {
        public Models.Portal Portal { get; set; }
        public List<Models.Role> Roles { get; set; }
        public List<Models.SecureActivity> SecureActivities { get; set; }
        public List<Models.User> Users { get; set; }
        //public List<Models.UserProfile> UserProfiles { get; set; }
        public List<Models.WidgetManifest> Manifests { get; set; }
        public List<Models.Localization> Localizations { get; set; }
        //public List<Models.Menu> Menus { get; set; }
        public List<Models.File> Files { get; set; }
        public List<Models.PageTemplate> PageTemplates { get; set; }

        [Obsolete("Use PageTemplates")]
        [SerializeIgnore(new string[] { "db", "client" })]
        public List<Models.PageTemplate> Templates 
        { 
            get 
            {
                return PageTemplates;
            }
            set 
            {
                PageTemplates = value;
            }
        } 
        public List<Models.LayoutTemplate> LayoutTemplates { get; set; }
        public Dictionary<string, string> WidgetContent { get; set; }
        public Dictionary<string, string> FileContent { get; set; }
        public List<Models.WebReference> WebReferences { get; set; }
        public List<Models.Menu> Menus { get; set; }
        
    }
}
