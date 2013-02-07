using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Videre.Core.Services;
using System.Web.Security;
using Videre.Core.ActionResults;
//using CodeEndeavors.Extensions;
//using Videre.Core.Extensions;
//using System.Diagnostics;

namespace Videre.Web.Controllers
{
    public class InstallerController : Controller
    {

        public JsonResult<dynamic> Install(Core.Models.User adminUser, Core.Models.Portal portal, List<string> packages)
        {
            return API.Execute<dynamic>(r =>
            {
                if (Portal.CurrentPortal == null)
                {
                    portal.Name = "Default";
                    portal.Default = true;

                    var packagesToInstall = Videre.Core.Services.Package.GetAvailablePackages().Where(p => packages.Contains(p.Name));
                    
                    //need to install account providers before we start creating accounts!
                    foreach (var package in packagesToInstall.Where(p => p.Type == "Account Provider"))
                        Package.InstallAvailablePackage(package.Name, package.Version, null);

                    var portalId = Core.Services.Update.InstallPortal(adminUser, portal);

                    foreach (var package in packagesToInstall.Where(p => p.Type != "Account Provider"))
                        Package.InstallAvailablePackage(package.Name, package.Version, portalId);

                }
                else 
                    r.AddError("Portal already exists.");   //todo: localize
            });

        }

    }
}
