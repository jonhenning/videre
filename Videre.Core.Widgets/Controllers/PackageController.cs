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

        public JsonResult<Dictionary<string, List<Models.Package>>> GetPackages()
        {
            return API.Execute<Dictionary<string, List<Models.Package>>>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                r.Data = GetPackageDict();
            });
        }

        public JsonResult<Dictionary<string, List<Models.Package>>> InstallPackage(string name, string version)
        {
            return API.Execute<Dictionary<string, List<Models.Package>>>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                CoreServices.Package.InstallAvailablePackage(name, version: version);
                r.Data = GetPackageDict();
            });
        }


        private Dictionary<string, List<Models.Package>> GetPackageDict()
        {
            return new Dictionary<string, List<Models.Package>>() 
                {
                    {"installedPackages", CoreServices.Package.GetInstalledPackages()},
                    {"availablePackages", CoreServices.Package.GetAvailablePackages()}
                };
        }

    }
}
