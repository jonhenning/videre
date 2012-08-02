using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Videre.Core.ActionResults;
using Videre.Core.Services;
//using DomainObjects = CodeEndeavors.ResourceManager.DomainObjects;
using Videre.Core.Extensions;
using Videre.Core;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Widgets.Controllers
{
    public class PageTemplateController : Controller
    {
        public JsonResult<List<CoreModels.PageTemplate>> Get()
        {
            return API.Execute<List<CoreModels.PageTemplate>>(r =>
            {
                r.Data = CoreServices.Portal.GetPageTemplates(); 
            });
        }

        public JsonResult<Dictionary<string, string>> GetWidgetContent(string templateId)
        {
            return API.Execute<Dictionary<string, string>>(r =>
            {
                var template = CoreServices.Portal.GetPageTemplateById(templateId);
                r.Data = template.GetWidgetContent();
            });
        }

        public JsonResult<bool> Save(CoreModels.PageTemplate template)
        {
            return API.Execute<bool>(r => {
                Security.VerifyActivityAuthorized("PageTemplate", "Administration");
                r.Data = !string.IsNullOrEmpty(CoreServices.Portal.Save(template));
            });
        }

        public JsonResult<bool> Delete(string id)
        {
            return API.Execute<bool>(r =>
            {
                Security.VerifyActivityAuthorized("PageTemplate", "Administration");
                r.Data = CoreServices.Portal.DeletePageTemplate(id);
            });
        }

    }
}
