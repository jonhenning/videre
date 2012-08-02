
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
    public class SearchController : Controller
    {
        public JsonResult<List<CoreModels.SearchResult>> Query(string term, int max = 8)
        {
            return API.Execute<List<CoreModels.SearchResult>>(r =>
            {
                r.Data = CoreServices.Search.Query(term);
                //if (r.Data.Count == 0)
                //    r.AddMessage(CoreServices.Localization.GetPortalText("NoResults.Text", "No results found"));
            });
        }

        public JsonResult<List<CoreModels.SearchProvider>> GetProviders()
        {
            return API.Execute<List<CoreModels.SearchProvider>>(r =>
            {
                r.Data = CoreServices.Search.GetSearchProviders(); 
            });
        }

        public JsonResult<bool> SaveProvider(CoreModels.SearchProvider provider)
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("Search", "Administration");
                r.Data = !string.IsNullOrEmpty(CoreServices.Search.Save(provider));
            });
        }

        public JsonResult<bool> DeleteProvider(string id)
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("Search", "Administration");
                r.Data = CoreServices.Search.DeleteSearchProvider(id);
            });
        }

        public JsonResult<bool> Generate(string id)
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("Search", "Administration");
                var messages = CoreServices.Search.Generate(id);
                foreach (var msg in messages)
                    r.AddMessage(msg);
                r.Data = true;
            });
        }

        public JsonResult<bool> ClearIndex()
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("Search", "Administration");
                CoreServices.Search.ClearIndex();
                r.Data = true;
            });
        }

    }
}
