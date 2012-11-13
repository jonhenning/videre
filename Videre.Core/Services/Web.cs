using System;
using System.Collections.Generic;
using System.Linq;
using Videre.Core.Models;

namespace Videre.Core.Services
{
    public class Web
    {
        public static List<Models.WebReference> GetDefaultWebReferences(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return new List<WebReference>()
            {
                new WebReference() { Name = "jQuery", Group = "jQuery", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/jquery-ui-1.8.18/js/jquery-1.7.1.min.js" },
                new WebReference() { Name = "jQuery UI", Group = "jQuery UI", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/jquery-ui-1.8.18/js/jquery-ui-1.8.18.custom.min.js", DependencyGroups = new List<string>() {"jQuery"} },
                new WebReference() { Name = "jQuery UI css", Group = "jQuery UI", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.StyleSheetReference, PortalId = portalId, Url = "~/scripts/jquery-ui-1.8.18/css/smoothness/jquery-ui-1.8.18.custom.css" },
                new WebReference() { Name = "jsRender", Group = "jsRender", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/jsrender.js" },
                new WebReference() { Name = "bootstrap", Group = "bootstrap", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/bootstrap-2.1.0/js/bootstrap.js" },

                new WebReference() { Name = "videre extensions", Group = "videre", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/videre.extensions.js", DependencyGroups = new List<string>() {"jQuery", "jQuery UI", "bootstrap", "jsRender"} , Sequence = 0 },
                new WebReference() { Name = "videre", Group = "videre", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/videre.js", Sequence = 1 },
                new WebReference() { Name = "videre css", Group = "videre", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.StyleSheetReference, PortalId = portalId, Url = "~/Content/videre.css" },
                
                new WebReference() { Name = "datatables", Group = "datatables", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/DataTables-1.9.0/media/js/jquery.dataTables.min.js", DependencyGroups = new List<string>() {"jQuery"}, Sequence = 0  },
                new WebReference() { Name = "datatables bootstrap", Group = "datatables", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/DataTables-1.9.0/media/js/jquery.dataTables.bootstrap.js", Sequence = 1 },
                new WebReference() { Name = "datatables css", Group = "datatables", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.StyleSheetReference, PortalId = portalId, Url = "~/scripts/DataTables-1.9.0/media/css/jquery.dataTables.bootstrap.css" },

                new WebReference() { Name = "prettify", Group = "prettify", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/prettify/prettify.js" },
                new WebReference() { Name = "prettify css", Group = "prettify", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.StyleSheetReference, PortalId = portalId, Url = "~/scripts/prettify/prettify.css" },
                new WebReference() { Name = "prettify startup", Group = "prettify", LoadType = WebReferenceLoadType.Startup, Type = WebReferenceType.Script, PortalId = portalId, Text = "prettyPrint();" },

                new WebReference() { Name = "dynatree", Group = "dynatree", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/dynatree1.1/jquery.dynatree.min.js", DependencyGroups = new List<string>() {"jQuery", "jQuery UI"}  },
                new WebReference() { Name = "dynatree css", Group = "dynatree", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.StyleSheetReference, PortalId = portalId, Url = "~/scripts/dynatree1.1/skin/ui.dynatree.css" },

                new WebReference() { Name = "timepicker", Group = "timepicker", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/jquery-ui-timepicker-1.0.1/jquery-ui-timepicker-addon.js", DependencyGroups = new List<string>() {"jQuery", "jQuery UI"} },
                new WebReference() { Name = "timepicker css", Group = "timepicker", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.StyleSheetReference, PortalId = portalId, Url = "~/scripts/jquery-ui-timepicker-1.0.1/jquery-ui-timepicker-addon.css", DependencyGroups = new List<string>() {"jQuery", "jQuery UI"} },

                new WebReference() { Name = "fileuploader", Group = "fileuploader", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/fileuploader.js", DependencyGroups = new List<string>() {"jQuery"} },

                new WebReference() { Name = "multiselect", Group = "multiselect", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/multiselect/jquery.multiselect.min.js", DependencyGroups = new List<string>() {"jQuery", "jQuery UI"} },
                new WebReference() { Name = "multiselect css", Group = "multiselect", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.StyleSheetReference, PortalId = portalId, Url = "~/scripts/multiselect/jquery.multiselect.css", DependencyGroups = new List<string>() {"jQuery", "jQuery UI"} },
                new WebReference() { Name = "multiselect startup", Group = "multiselect", LoadType = WebReferenceLoadType.Startup, Type = WebReferenceType.Script, PortalId = portalId, Text = "$('select[data-controltype=\"multiselect\"]').multiselect({ selectedList: 3 });" },

                new WebReference() { Name = "CLTextEditor", Group = "CLTextEditor", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/controls/core/CLTextEditor/CLEditor1_3_0/jquery.cleditor.min.js", DependencyGroups = new List<string>() {"jQuery"} },
                new WebReference() { Name = "CLTextEditor css", Group = "CLTextEditor", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.StyleSheetReference, PortalId = portalId, Url = "~/scripts/controls/core/CLTextEditor/CLEditor1_3_0/jquery.cleditor.css", DependencyGroups = new List<string>() {"jQuery"} },

                new WebReference() { Name = "CKTextEditor", Group = "CKTextEditor", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/controls/core/CKTextEditor/ckeditor_3.6.4/ckeditor.js", DependencyGroups = new List<string>() {"jQuery"} },
                new WebReference() { Name = "CKTextEditor jQuery adapter", Group = "CKTextEditor", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/controls/core/CKTextEditor/ckeditor_3.6.4/adapters/jquery.js", DependencyGroups = new List<string>() {"jQuery"} },

                new WebReference() { Name = "WYSIHTML5", Group = "WYSIHTML5TextEditor", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/controls/core/WYSIHTML5TextEditor/WYSIHTML5_0.2/lib/js/wysihtml5-0.3.0.min.js", DependencyGroups = new List<string>() {"jQuery"} },
                new WebReference() { Name = "WYSIHTML5 bootstrap", Group = "WYSIHTML5TextEditor", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/controls/core/WYSIHTML5TextEditor/WYSIHTML5_0.2/dist/bootstrap-wysihtml5-0.0.2.min.js", DependencyGroups = new List<string>() {"jQuery"} },
                new WebReference() { Name = "WYSIHTML5 css", Group = "WYSIHTML5TextEditor", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.StyleSheetReference, PortalId = portalId, Url = "~/scripts/controls/core/WYSIHTML5TextEditor/WYSIHTML5_0.2/dist/bootstrap-wysihtml5-0.0.2.css", DependencyGroups = new List<string>() {"jQuery"} },

                new WebReference() { Name = "highcharts", Group = "highcharts", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/controls/core/highcharts/highcharts.js", DependencyGroups = new List<string>() {} }

            };
        }

        public static List<Models.WebReference> GetWebReferences(string portalId = null, bool compat = false)    
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            var refs = Repository.Current.GetResources<Models.WebReference>("WebReference").Select(m => m.Data).Where(r => r.PortalId.Equals(portalId, StringComparison.InvariantCultureIgnoreCase)).OrderBy(r => r.Group).ToList();
            if (refs.Count == 0 || compat)    
                refs = GetDefaultWebReferences(portalId);
            return refs;
        }
        public static Models.WebReference GetWebReference(string portalId, string name)
        {
            return GetWebReferences(portalId).Where(r => r.PortalId.Equals(portalId, StringComparison.InvariantCultureIgnoreCase) && name.Equals(r.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
        public static Models.WebReference GetWebReferenceById(string id)
        {
            return GetWebReferences().Where(r => r.Id == id).FirstOrDefault();
        }
        public static string Import(string portalId, Models.WebReference webReference, Dictionary<string, string> idMap, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = GetWebReference(portalId, webReference.Name);
            webReference.Id = existing != null ? existing.Id : null;
            webReference.PortalId = portalId;
            return Save(webReference, userId);
        }
        public static string Save(Models.WebReference webReference, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            webReference.PortalId = string.IsNullOrEmpty(webReference.PortalId) ? Portal.CurrentPortalId : webReference.PortalId;

            Validate(webReference);
            var res = Repository.Current.StoreResource("WebReference", null, webReference, userId);
            return res.Id;
        }
        public static void Validate(Models.WebReference webReference)
        {
            if (string.IsNullOrEmpty(webReference.PortalId) || string.IsNullOrEmpty(webReference.Name) ||
                ((webReference.Type == WebReferenceType.ScriptReference || webReference.Type == WebReferenceType.StyleSheetReference) && string.IsNullOrEmpty(webReference.Url)) ||
                ((webReference.Type == WebReferenceType.Script || webReference.Type == WebReferenceType.StyleSheet) && string.IsNullOrEmpty(webReference.Text)))
                throw new Exception(Localization.GetExceptionText("InvalidResource.Error", "{0} is invalid.", "WebReference"));
            if (IsDuplicate(webReference))
                throw new Exception(Localization.GetExceptionText("DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "WebReference"));
        }
        public static bool IsDuplicate(Models.WebReference webReference)
        {
            var e = GetWebReference(webReference.PortalId, webReference.Name);
            return (e != null && e.Id != webReference.Id);
        }
        public static bool Exists(Models.WebReference webReference)
        {
            return  GetWebReference(webReference.PortalId, webReference.Name) != null;
        }
        public static bool DeleteWebReference(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var res = Repository.Current.GetResourceById<Models.WebReference>(id);
            if (res != null)
                Repository.Current.Delete(res);
            return res != null;
        }

    }
}
