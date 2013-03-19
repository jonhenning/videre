using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainObjects = CodeEndeavors.ResourceManager.DomainObjects;
using Videre.Core.Models;
using CodeEndeavors.Extensions;

namespace Videre.Core.Services
{
    public class Comment
    {
        public static List<Models.Comment> GetComments(string containerType, string containerId, string portalId = null)
        {
            var ret = new List<Models.Comment>();
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            var container = Get(containerType, containerId, portalId);
            if (container != null && container.Comments != null)
                ret = container.Comments;
            return ret;
        }

        public static Models.CommentContainer SaveComment(Models.Comment comment, string containerType, string containerId, string portalId = null, string userId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var container = Get(containerType, containerId, portalId);
            if (container == null)
                container = new CommentContainer() { ContainerType = containerType, ContainerId = containerId, PortalId = portalId };
            
            //TODO: validate comment!
            var existing = container.Comments.Where(c => c.Id == comment.Id).FirstOrDefault();
            if (existing == null)
            {
                comment.Id = Guid.NewGuid().ToString(); //use guid or have counter on container?
                comment.CreatedDate = DateTime.UtcNow;
                container.Comments.Add(comment);
            }
            else
            {
                comment.Id = existing.Id;
                //todo: easy way to replace an element in collection instead of this copy?
                existing.Text = comment.Text;
                existing.Email = comment.Email;
                existing.ApprovedDate = comment.ApprovedDate;
                existing.CreatedDate = comment.CreatedDate;
            }

            Save(container, userId);
            return container;
        }

        public static Models.CommentContainer ApproveComment(string commentId, string containerType, string containerId, string portalId = null, string userId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var container = Get(containerType, containerId, portalId);
            if (container == null)
                container = new CommentContainer() { ContainerType = containerType, ContainerId = containerId, PortalId = portalId };

            var comment = container.Comments.Where(c => c.Id == commentId).FirstOrDefault();
            if (comment != null)
            {
                comment.ApprovedDate = DateTime.UtcNow;
                Save(container, userId);
            }
            else
                throw new Exception("Comment not found");   //todo:  exception?
            return container;
        }

        public static Models.CommentContainer RemoveComment(string commentId, string containerType, string containerId, string portalId = null, string userId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var container = Get(containerType, containerId, portalId);
            if (container == null)
                container = new CommentContainer() { ContainerType = containerType, ContainerId = containerId, PortalId = portalId };

            var comment = container.Comments.Where(c => c.Id == commentId).FirstOrDefault();
            if (comment != null)
            {
                container.Comments.Remove(comment);
                Save(container, userId);
            }
            else
                throw new Exception("Comment not found");   //todo:  exception?
            return container;
        }

        public static Models.CommentContainer GetById(string id)
        {
            var res = Repository.Current.GetResourceById<Models.CommentContainer>(id);
            if (res != null)
                return res.Data;
            return null;
        }
        
        public static List<Models.CommentContainer> Get(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return Repository.Current.GetResources<Models.CommentContainer>("CommentContainer", m => m.Data.PortalId == portalId, false).Select(f => f.Data).ToList();
        }

        public static Models.CommentContainer Get(string containerType, string containerId, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return Repository.Current.GetResourceData<Models.CommentContainer>("CommentContainer", m => m.Data.PortalId == portalId && m.Data.ContainerType == containerType && m.Data.ContainerId == containerId, null);
        }

        public static string Import<T>(string portalId, Models.CommentContainer container, Dictionary<string, string> idMap, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Get(container.ContainerType, container.ContainerId, portalId);
            if (existing != null)
                container.Id = existing.Id;
            else
                container.Id = null;
            container.PortalId = portalId;
            container.ContainerId = ImportExport.GetIdMap<T>(container.ContainerId, idMap);
            return Save(container, userId);
        }

        public static string Save(Models.CommentContainer container, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            container.PortalId = string.IsNullOrEmpty(container.PortalId) ? Services.Portal.CurrentPortalId : container.PortalId;
            Validate(container);
            var res = Repository.Current.StoreResource("CommentContainer", null, container, userId);
            return res.Id;
        }

        public static void Validate(Models.CommentContainer container)
        {
            if (string.IsNullOrEmpty(container.ContainerType) || string.IsNullOrEmpty(container.ContainerId))
                throw new Exception(Localization.GetExceptionText("InvalidResource.Error", "{0} is invalid.", "CommentContainer"));
            if (IsDuplicate(container))
                throw new Exception(Localization.GetExceptionText("DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "CommentContainer"));
        }

        public static bool IsDuplicate(Models.CommentContainer container)
        {
            var r = Get(container.ContainerType, container.ContainerId, container.PortalId);
            if (r != null)
                return r.Id != container.Id;
            return false;
        }

        public static bool Exists(Models.CommentContainer container)
        {
            var existing = Get(container.ContainerType, container.ContainerId, container.PortalId); 
            return existing != null && (existing.ContainerType == container.ContainerType && existing.ContainerId == container.ContainerId);
        }

        public static bool Delete(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var container = Repository.Current.GetResourceById<Models.CommentContainer>(id);
            if (container != null)
                Repository.Current.Delete(container);
            return container != null;
        }

    }
}
