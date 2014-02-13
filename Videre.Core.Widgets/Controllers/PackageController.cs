using System;
using System.Linq;
using CodeEndeavors.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Videre.Core.ActionResults;
using Videre.Core.Services;
using Videre.Core.Extensions;
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

        public JsonResult<Dictionary<string, List<Models.Package>>> UninstallPackage(string name, string version)
        {
            return API.Execute<Dictionary<string, List<Models.Package>>>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                CoreServices.Package.UninstallPackage(name, version);
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
            catch 
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

        public JsonResult<List<Models.ImportExportContent>> GetExportContentItems(string type, Models.PortalExport exportPackage)
        {
            return API.Execute<List<Models.ImportExportContent>>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                r.Data = ImportExport.GetProvider(type).GetExportContentItems(exportPackage);
            });

        }

        public JsonResult<Models.PortalExport> ExportContentItem(string type, string id, Models.PortalExport exportPackage)
        {
            return API.Execute<Models.PortalExport>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                r.Data = ImportExport.GetProvider(type).Export(id, exportPackage);
            });

        }

        //public FileContentResult ExportPackage(Models.Package manifest, Models.PortalExport export)
        [ValidateInput(false)]
        public FileContentResult ExportPackage(string manifest, string exportPackage)
        {
            var m = manifest.ToObject<Models.Package>();
            m.PackagedDate = DateTime.Now;

            var content = exportPackage.ToObject<Models.PortalExport>();

            var ret =  File(new Dictionary<string, string>()
                {
                    {"package.manifest", m.ToJson(true, "db") },
                    {"content.json", content.ToJson(true, "db") }
                }.ZipToByteArray(), "application/zip");
            ret.FileDownloadName = m.FileName;

            return ret;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult<dynamic> GetPackageContents(string qqfile)
        {
            return API.Execute<dynamic>(r =>
            {
                r.ContentType = "text/html";
                string fileName = null;
                //var tempFileName = Portal.GetTempFile(CoreServices.Portal.CurrentPortalId);
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

                var manifest = stream.GetZipEntryContents("package.manifest");
                var content = stream.GetZipEntryContents("content.json");
                r.Data = new
                {
                    manifest = manifest != null ? manifest.ToObject<Models.Package>() : null,
                    content = content != null ? content.ToObject<Models.PortalExport>() : null
                };
            }, false);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult<dynamic> ImportPackage(string qqfile)
        {
            return API.Execute<dynamic>(r =>
            {
                r.ContentType = "text/html";
                string fileName = null;
                //var tempFileName = Portal.GetTempFile(CoreServices.Portal.CurrentPortalId);
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
                var saveFileName = Portal.TempDir + fileName;
                if (Web.MimeTypes.ContainsKey(ext))
                {
                    if (ext.Equals("json", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string json = null;
                        using (var reader = new StreamReader(stream))
                        {
                            json = reader.ReadToEnd();
                        }
                        var portalExport = json.ToObject<Models.PortalExport>();
                        Services.ImportExport.Import(portalExport, Portal.CurrentPortalId);
                        r.AddMessage(Localization.GetPortalText("DataImportMessage.Text", "Data has been imported successfully.  You may need to refresh your page to see changes."));
                    }
                    else if (ext.Equals("zip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var fileSize = stream.WriteStream(saveFileName);
                        r.Data = new
                        {
                            uniqueName = new FileInfo(saveFileName).Name,
                            fileName = fileName,
                            fileSize = fileSize,
                            mimeType = Web.MimeTypes[ext]
                        };
                        Package.InstallFile(saveFileName, portalId: Portal.CurrentPortalId,  removeFile: true);

                        r.AddMessage(Localization.GetPortalText("ImportMessage.Text", "File has been installed successfully"));
                    }
                }
                else
                    throw new Exception(Localization.GetExceptionText("InvalidMimeType.Error", "{0} is invalid.", ext));
            }, false);
        }


    }
}
