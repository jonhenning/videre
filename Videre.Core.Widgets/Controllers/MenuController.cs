using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Videre.Core.ActionResults;
using Videre.Core.Services;
using CodeEndeavors.Extensions;
using System.IO;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;
//using DomainObjects = CodeEndeavors.ResourceManager.DomainObjects;

namespace Videre.Core.Widgets.Controllers
{
    public class MenuController : Controller
    {
        public JsonResult<List<CoreModels.Menu>> Get()
        {
            return API.Execute<List<CoreModels.Menu>>(r =>
            {
                r.Data = CoreServices.Menu.Get();
            });
        }

        //public JsonResult<List<CoreModels.Menu>> Save(CoreModels.Menu menu)
        //{
        //    return API.Execute<List<CoreModels.Menu>>(r =>
        //    {
        //        Security.VerifyActivityAuthorized("Portal", "Administration");
        //        CoreServices.Menu.Save(menu);
        //        r.Data = CoreServices.Menu.Get();
        //    });
        //}

        public JsonResult<List<CoreModels.Menu>> Delete(string id)
        {
            return API.Execute<List<CoreModels.Menu>>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");    //todo:  have own area for menu maint or use portal?
                CoreServices.Menu.Delete(id);
                r.Data = CoreServices.Menu.Get();
            });
        }
    }
}
