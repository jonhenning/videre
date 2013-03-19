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
    public class WidgetController : Controller
    {

        public JsonResult<List<CoreModels.WidgetManifest>> GetManifests()
        {
            return API.Execute<List<CoreModels.WidgetManifest>>(r =>
            {
                r.Data = CoreServices.Widget.GetWidgetManifests();
            });
        }

        //public JsonResult GetManifestsPaged(string sidx, string sord, int page, int rows)
        //{
        //    var manifests = CoreServices.Portal.GetWidgetManifests();
        //    int pageIndex = Convert.ToInt32(page) - 1;
        //    int pageSize = rows;
        //    int totalRecords = manifests.Count;
        //    int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);

        //    //manifests = manifests.OrderBy(sidx + " " + sord).Skip(pageIndex * pageSize).Take(pageSize);
        //    manifests = manifests.Skip(pageIndex * pageSize).Take(pageSize).ToList();

        //    return Json(new
        //    {
        //        total = totalPages,
        //        page,
        //        records = totalRecords,
        //        rows = (
        //            from manifest in manifests
        //            select new
        //            {
        //                i = manifest.Id,
        //                cell = new string[] { manifest.Category, manifest.Name, manifest.Title }
        //            }).ToArray()
        //    });

        //}

        public JsonResult<bool> SaveManifest(CoreModels.WidgetManifest manifest)
        {
            return API.Execute<bool>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                r.Data = !string.IsNullOrEmpty(CoreServices.Widget.Save(manifest));
            });
        }

        public JsonResult<bool> DeleteManifest(string id)
        {
            return API.Execute<bool>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                r.Data = CoreServices.Widget.DeleteManifest(id);
            });
        }



    }
}
