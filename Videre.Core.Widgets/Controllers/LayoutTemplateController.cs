using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Videre.Core.ActionResults;
using Videre.Core.Services;
//using DomainObjects = CodeEndeavors.ResourceManager.DomainObjects;
using Videre.Core.Extensions;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Widgets.Controllers
{
    public class LayoutTemplateController : Controller
    {
        public JsonResult<List<CoreModels.LayoutTemplate>> Get()
        {
            return API.Execute<List<CoreModels.LayoutTemplate>>(r =>
            {
                r.Data = CoreServices.Portal.GetLayoutTemplates(CoreServices.Portal.CurrentPortalId);
            });
        }

        public JsonResult<Dictionary<string, string>> GetWidgetContent(string templateId)
        {
            return API.Execute<Dictionary<string, string>>(r =>
            {
                var template = CoreServices.Portal.GetLayoutTemplateById(templateId);
                r.Data = template.GetWidgetContent();
            });
        }

        public JsonResult<bool> Save(CoreModels.LayoutTemplate template)
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("LayoutTemplate", "Administration");
                r.Data = !string.IsNullOrEmpty(CoreServices.Portal.Save(template));
            });
        }

        public JsonResult<bool> Delete(string id)
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("LayoutTemplate", "Administration");
                r.Data = CoreServices.Portal.DeleteLayoutTemplate(id);
            });
        }

    }
}
