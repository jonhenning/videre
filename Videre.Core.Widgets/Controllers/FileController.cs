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
    public class FileController : Controller
    {
        public JsonResult<List<CoreModels.File>> Get()
        {
            return API.Execute<List<CoreModels.File>>(r =>
            {
                r.Data = CoreServices.File.Get(CoreServices.Portal.CurrentPortalId);
            });
        }

        public JsonResult<bool> Save(CoreModels.File file, string uniqueName)
        {
            return API.Execute<bool>(r =>
            {
                if (!string.IsNullOrEmpty(file.Id) || !string.IsNullOrEmpty(uniqueName))
                {
                    CoreServices.Security.VerifyActivityAuthorized("File", "Administration");
                    var fileId = CoreServices.File.Save(file);
                    if (!string.IsNullOrEmpty(uniqueName))
                    {
                        var newFileName = Portal.GetFile(CoreServices.Portal.CurrentPortalId, fileId);
                        if (System.IO.File.Exists(newFileName))
                            System.IO.File.Delete(newFileName);
                        System.IO.File.Move(Portal.GetTempFile(CoreServices.Portal.CurrentPortalId, uniqueName), newFileName);
                    }
                    r.Data = !string.IsNullOrEmpty(fileId);
                }
                else
                    throw new Exception(Localization.GetExceptionText("InvalidFile.Error", "In order to save, file must exist."));
            });
        }

        public FilePathResult View(string Url)
        {
            FilePathResult result = null;
            var file = CoreServices.File.Get(CoreServices.Portal.CurrentPortalId, Url);
            if (file != null)
            {
                var fileName = Portal.GetFile(CoreServices.Portal.CurrentPortalId, file.Id);
                result = File(fileName, file.MimeType);

                this.Response.Cache.SetCacheability(System.Web.HttpCacheability.Public);
                this.Response.AddFileDependency(fileName);
                this.Response.Cache.SetLastModifiedFromFileDependencies();
            
            }

            return result;
        }

        public JsonResult<bool> Delete(string id)
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("File", "Administration");
                r.Data = CoreServices.File.Delete(id, CoreServices.Account.CurrentUser.Id);
            });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult<dynamic> Upload(string qqfile)
        {
            return API.Execute<dynamic>(r =>
            {
                r.ContentType = "text/html";
                string fileName = null;
                var tempFileName = Portal.GetTempFile(CoreServices.Portal.CurrentPortalId);
                System.IO.Stream stream = null;
                if (string.IsNullOrEmpty(Request["qqfile"]))    //IE
                {
                    fileName = Request.Files[0].FileName;
                    stream = Request.Files[0].InputStream;
                }
                else
                {
                    fileName = qqfile;
                    stream = Request.InputStream;
                }
                var ext = fileName.Substring(fileName.LastIndexOf(".") + 1);
                if (Web.MimeTypes.ContainsKey(ext) && IsMimeTypeAllowed(ext))
                {
                    var fileSize = stream.WriteStream(tempFileName);
                    r.Data = new
                    {
                        uniqueName = new FileInfo(tempFileName).Name,
                        fileName = fileName,
                        fileSize = fileSize,
                        mimeType = Web.MimeTypes[ext]
                    };
                }
                else
                    throw new Exception(Localization.GetExceptionText("InvalidMimeType.Error", "{0} is invalid.", ext));
            });
        }

        public static bool IsMimeTypeAllowed(string MimeType)   //todo: secure this!
        {
            return true;
        }

    }
}
