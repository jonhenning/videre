﻿using System;
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
                        var newFileName = Portal.GetFile(fileId);
                        if (System.IO.File.Exists(newFileName))
                            System.IO.File.Delete(newFileName);
                        System.IO.File.Move(Portal.GetTempFile(fileName: uniqueName), newFileName);
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
                var fileName = Portal.GetFile(file.Id);
                result = File(fileName, file.MimeType);
                
                this.Response.AddFileDependency(fileName);
                this.Response.Cache.SetCacheability(System.Web.HttpCacheability.Public);
                this.Response.Cache.SetLastModifiedFromFileDependencies();
                this.Response.Cache.SetETagFromFileDependencies();
                
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
                var response = new List<object>();

                if (Request.Files.Count > 0)
                {
                    foreach (string uploadName in Request.Files)
                        response.Add(processFile(Request.Files[uploadName].FileName, Request.Files[uploadName].InputStream));
                }
                else
                    response.Add(processFile(qqfile, Request.InputStream));

                r.Data = response;
            }, false);
        }

        private static object processFile(string fileName, Stream stream)
        {
            var tempFileName = Portal.GetTempFile();
            var ext = fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower();
            if (Videre.Core.Web.MimeTypes.ContainsKey(ext) && IsMimeTypeAllowed(ext))
            {
                var fileSize = stream.WriteStream(tempFileName);
                return new
                {
                    UniqueName = new FileInfo(tempFileName).Name,
                    FileName = new FileInfo(fileName).Name,
                    Size = fileSize,
                    MimeType = Videre.Core.Web.MimeTypes[ext]
                };
            }
            else
                throw new Exception(Localization.GetExceptionText("InvalidMimeType.Error", "{0} is invalid.", ext));
        }


        public static bool IsMimeTypeAllowed(string MimeType)   //todo: secure this!
        {
            return true;
        }

    }
}
