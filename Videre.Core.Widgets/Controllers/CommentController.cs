using System.Linq;
using System.Web.Mvc;
using Videre.Core.ActionResults;
using System.Collections.Generic;
using Videre.Core.Services;
using System.Web.Security;
using System;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Widgets.Controllers
{
    public class CommentController : Controller
    {
        public JsonResult<CoreModels.CommentContainer> AddComment(CoreModels.Comment comment, string containerType, string containerId)
        {
            return API.Execute<CoreModels.CommentContainer>(r =>
            {
                //todo: strip html?
                //comment.Official = false;
                comment.ApprovedDate = null;
                comment.Id = null;

                if (!string.IsNullOrEmpty(comment.Email))
                    CoreServices.Validation.ValidateEmail(comment.Email);
                r.Data = CoreServices.Comment.SaveComment(comment, containerType, containerId);
                r.AddMessage(Localization.GetPortalText("CommentAdded.Text", "Comment has been posted.  It will show up here upon approval."));
            });
        }

        public JsonResult<CoreModels.CommentContainer> ApproveComment(string id, string containerType, string containerId)
        {
            return API.Execute<CoreModels.CommentContainer>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("Blog", "Administration");
                r.Data = CoreServices.Comment.ApproveComment(id, containerType, containerId);
            });
        }

        public JsonResult<CoreModels.CommentContainer> RemoveComment(string id, string containerType, string containerId)
        {
            return API.Execute<CoreModels.CommentContainer>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("Blog", "Administration");
                r.Data = CoreServices.Comment.RemoveComment(id, containerType, containerId);
            });
        }

    }
}
