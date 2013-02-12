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

        public JsonResult<Dictionary<string, List<Models.Package>>> RemovePackage(string name, string version)
        {
            return API.Execute<Dictionary<string, List<Models.Package>>>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                CoreServices.Package.RemoveAvailablePackage(name, version);
                r.Data = GetPackageDict();
            });
        }

        public JsonResult<Dictionary<string, List<Models.Package>>> TogglePublishPackage(string name, string version, bool publish)
        {
            return API.Execute<Dictionary<string, List<Models.Package>>>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                CoreServices.Package.TogglePublishPackage(name, version, publish);
                r.Data = GetPackageDict();
            });
        }

        public JsonResult<Dictionary<string, List<Models.Package>>> DownloadPackage(string name, string version)
        {
            return API.Execute<Dictionary<string, List<Models.Package>>>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                CoreServices.Package.DownloadRemotePackage(name, version);
                r.Data = GetPackageDict();
            });
        }

        private Dictionary<string, List<Models.Package>> GetPackageDict()
        {
            var remotePackages = new List<Models.Package>();
            try
            {
                remotePackages = CoreServices.Package.GetRemotePackages();
            }
            catch (Exception ex)
            {
                //ignore?
            }
            return new Dictionary<string, List<Models.Package>>() 
                {
                    {"installedPackages", CoreServices.Package.GetInstalledPackages()},
                    {"availablePackages", CoreServices.Package.GetAvailablePackages()},
                    {"publishedPackages", CoreServices.Package.GetPublishedPackages()},
                    {"remotePackages", remotePackages},
                };
        }

        //public JsonResult<List<Models.Package>> GetPublishedPackages()
        public string GetPublishedPackages()    //todo: why use json result to begin with?!?!?!
        {
            var result =  API.Execute<List<Models.Package>>(r =>
            {
                r.Data = CoreServices.Package.GetPublishedPackages();
            });
            return result.Data.ToJson();
        }

        public FilePathResult GetPublishedPackage(string name, string version)
        {
            //var data = url.Split('/');

            FilePathResult result = null;
            var package = CoreServices.Package.GetPublishedPackage(name, version);
            if (package != null)
                result = File(package.FileName, "application/zip");
            return result;
        }

    }
}
