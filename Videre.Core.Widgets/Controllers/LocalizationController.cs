using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using Videre.Core.ActionResults;
using Videre.Core.Services;
//using DomainObjects = CodeEndeavors.ResourceManager.DomainObjects;
using CodeEndeavors.Extensions;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Widgets.Controllers
{
    public class LocalizationController : Controller
    {
        public JsonResult<List<CoreModels.Localization>> Get(string Type)
        {
            return API.Execute<List<CoreModels.Localization>>(r => {
                r.Data = CoreServices.Localization.Get(Type.ToObject<CoreModels.LocalizationType>(), CoreServices.Portal.CurrentPortalId); 
            });
        }

        public JsonResult<bool> Save(CoreModels.Localization Localization)
        {
            return API.Execute<bool>(r => {
                CoreServices.Security.VerifyActivityAuthorized("Localization", "Administration");
                r.Data = CoreServices.Localization.Save(Localization) != null;
            });
        }

        public JsonResult<bool> Delete(string id)
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("Localization", "Administration");
                r.Data = CoreServices.Localization.Delete(id);
            });
        }

        public JsonResult<object> GetSharedWidgetContent()
        {
            return API.Execute<object>(r =>
            {
                var localizations = CoreServices.Localization.Get(CoreModels.LocalizationType.WidgetContent, CoreServices.Portal.CurrentPortalId).Where(l => !string.IsNullOrEmpty(l.Namespace) && !l.Namespace.StartsWith("__")).ToList(); //todo: here is the mini-hack identifiying shared content from single instance.  See LocalizationContentProvider for other half
                var locDict = localizations.ToDictionary(l => l.Id);
                var dict = new Dictionary<string, List<string>>();  //[contentId, List<widgetId>]
                //get all content ids
                var widgets = CoreServices.Portal.GetPageTemplates().SelectMany(t => t.Widgets).Where(w => w.ContentIds.Count > 0).ToList();
                widgets.AddRange(CoreServices.Portal.GetLayoutTemplates().SelectMany(t => t.Widgets).Where(w => w.ContentIds.Count > 0));
                foreach (var widget in widgets)
                {
                    foreach (var contentId in widget.ContentIds)
                    {
                        if (locDict.ContainsKey(contentId)) //only include items that have namespaces
                        {
                            if (!dict.ContainsKey(contentId))
                                dict[contentId] = new List<string>();
                            dict[contentId].Add(widget.Id);
                        }
                    }
                }

                //var contentIds = CoreServices.Portal.GetTemplates().SelectMany(t => t.Widgets).SelectMany(w => w.ContentIds).ToList();
                //contentIds.AddRange(CoreServices.Portal.GetLayoutTemplates().SelectMany(t => t.Widgets).SelectMany(w => w.ContentIds));
                //var idCounts = contentIds.GroupBy(i => i).Select(i => new { Id = i.Key, Count = i.Count() });
                r.Data = new { localizations = localizations, idCounts = dict };
            });
        }

    }
}
