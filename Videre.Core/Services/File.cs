using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainObjects = CodeEndeavors.ResourceManager.DomainObjects;
using Videre.Core.Models;
using CodeEndeavors.Extensions;

namespace Videre.Core.Services
{
    public class File
    {
        public static List<Models.File> GetByMimeType(string mimeType, string portalId = null)
        {
            return Get(portalId).Where(f => string.IsNullOrEmpty(mimeType) || f.MimeType.IndexOf(mimeType,  StringComparison.InvariantCultureIgnoreCase) > -1).ToList();
        }

        public static List<Models.File> Get(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return Repository.GetResources<Models.File>("File", f => f.Data.PortalId == portalId, false).Select(f => f.Data).ToList();
        }

        public static Models.File Get(string portalId, string url)
        {
            var fileResource = Repository.GetResources<Models.File>("File", f => f.Data.PortalId == portalId && (f.Data.Url == url || f.Data.RenderUrl == url)).SingleOrDefault();
            if (fileResource != null)
                return fileResource.Data; //.ToModel();
            return null;
            //return Repository.Get<Models.File>("File", f => f.PortalId == PortalId && f.Urls.Contains(Url), null);
        }

        public static Models.File GetById(string id)
        {
            var fileResource = Repository.GetResourceById<Models.File>(id);
            if (fileResource != null)
                return fileResource.Data; //.ToModel();
            return null;
            //return Repository.Get<Models.File>("File", f => f.PortalId == PortalId && f.Urls.Contains(Url), null);
        }

        public static string Save(Models.File file, string userId = null)
        {
            file.PortalId = string.IsNullOrEmpty(file.PortalId) ? Portal.CurrentPortalId : file.PortalId;
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            Validate(file);
            var res = Repository.StoreResource("File", null, file, userId);
            return res.Id;
        }

        public static void Validate(Models.File file)
        {
            if (string.IsNullOrEmpty(file.Url) || string.IsNullOrEmpty(file.MimeType))
                throw new Exception(Localization.GetExceptionText("InvalidResource.Error", "{0} is invalid.", "File"));
            if (IsDuplicate(file))
                throw new Exception(Localization.GetExceptionText("DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "File"));
        }

        private static bool IsDuplicate(Models.File File)
        {
            var file = Get(File.PortalId, File.Url); 
            return file != null && file.Id != File.Id;
            //foreach (var url in File.Urls)
            //{
            //    var file = Get(File.PortalId, url);
            //    if (file != null && file.Id != File.Id)
            //        return true;
            //}
            //return false;
        }

        public static bool Delete(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var file = Repository.GetResourceById<Models.File>(id);
            if (file != null)
                Repository.Delete(file);
            return file != null;
        }



    }
}
