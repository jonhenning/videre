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
                new WebReference() { Name = "videre extensions", Group = "videre", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/videre.extensions.js", DependencyGroups = new List<string>() {"jQuery", "jQuery UI", "bootstrap", "jsRender", "moment"} , Sequence = 0 },
                new WebReference() { Name = "videre", Group = "videre", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.ScriptReference, PortalId = portalId, Url = "~/scripts/videre.js", Sequence = 1 },
                new WebReference() { Name = "videre css", Group = "videre", LoadType = WebReferenceLoadType.Defer, Type = WebReferenceType.StyleSheetReference, PortalId = portalId, Url = "~/Content/videre.css" }
            };
        }

        public static List<Models.WebReference> GetWebReferences(string portalId = null)    
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return Repository.Current.GetResources<Models.WebReference>("WebReference").Select(m => m.Data).Where(r => r.PortalId.Equals(portalId, StringComparison.InvariantCultureIgnoreCase)).OrderBy(r => r.Group).ToList();
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
            var portalId = string.IsNullOrEmpty(webReference.PortalId) ? Portal.CurrentPortalId : webReference.PortalId;
            return  GetWebReference(portalId, webReference.Name) != null;
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
