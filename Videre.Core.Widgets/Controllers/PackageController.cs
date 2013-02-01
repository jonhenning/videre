using System;
using System.Linq;
using CodeEndeavors.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Videre.Core.ActionResults;
using Videre.Core.Services;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Widgets.Controllers
{
    public class PackageController : Controller
    {

        public JsonResult<List<CoreModels.Package>> GetInstalledPackages()
        {
            return API.Execute<List<CoreModels.Package>>(r =>
            {
                r.Data = CoreServices.Package.GetInstalledPackages();
            });
        }

        public JsonResult<List<CoreModels.Package>> GetAvailablePackages()
        {
            return API.Execute<List<CoreModels.Package>>(r =>
            {
                r.Data = CoreServices.Package.GetAvailablePackages();
            });
        }

    }
}
