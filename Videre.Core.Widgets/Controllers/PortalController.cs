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
    public class PortalController : Controller
    {
        public JsonResult<dynamic> CreatePortal(CoreModels.User adminUser, CoreModels.Portal portal, List<string> packages)
        {
            return API.Execute<dynamic>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                var portalId = Core.Services.Update.InstallPortal(adminUser, portal);

                foreach (var package in packages)
                    Package.InstallAvailablePackage(package, portalId: portalId);
                r.Data = new { selectedId = portalId, portals = CoreServices.Portal.GetPortals() };
            });
        }

        public JsonResult<dynamic> SavePortal(CoreModels.Portal portal)
        {
            return API.Execute<dynamic>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                if (CoreServices.Portal.Save(portal))
                {
                    r.Data = new { selectedId = portal.Id, portals = CoreServices.Portal.GetPortals() };
                    r.AddMessage(Localization.GetPortalText("DataSave.Text", "Data has been saved."));
                }
            });
        }

        public JsonResult<List<CoreModels.SecureActivity>> GetSecureActivities()
        {
            return API.Execute<List<CoreModels.SecureActivity>>(r =>
            {
                r.Data = CoreServices.Security.GetSecureActivities();
            });
        }

        public JsonResult<bool> SaveSecureActivity(CoreModels.SecureActivity activity)
        {
            return API.Execute<bool>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                r.Data = !string.IsNullOrEmpty(CoreServices.Security.Save(activity));
            });
        }

        public JsonResult<bool> DeleteSecureActivity(string id)
        {
            return API.Execute<bool>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                r.Data = CoreServices.Security.DeleteSecureActivity(id);
            });
        }

        public FileContentResult ExportPortal(string id)
        {
            Security.VerifyActivityAuthorized("Portal", "Administration");
            var export = CoreServices.Portal.ExportPortal(id);
            var json = export.ToJson(pretty: true, ignoreType: "db");   //todo: use db for export, or its own type?
            return File(System.Text.Encoding.UTF8.GetBytes(json), "text/plain", "ExportPortal.json");
        }

        public FileContentResult ExportTemplates(string id)
        {
            Security.VerifyActivityAuthorized("Portal", "Administration");
            var export = CoreServices.Portal.ExportTemplates(id, true);
            var json = export.ToJson(pretty: true, ignoreType: "db");   //todo: use db for export, or its own type?
            return File(System.Text.Encoding.UTF8.GetBytes(json), "text/plain", "ExportPortalTemplates.json");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult<dynamic> ImportPortal(string qqfile)
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
                        Services.Portal.Import(portalExport, Portal.CurrentPortalId);
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
                        Package.InstallFile(saveFileName, removeFile: true);
                        
                        r.AddMessage(Localization.GetPortalText("ImportMessage.Text", "File has been installed successfully"));
                    }
                }
                else
                    throw new Exception(Localization.GetExceptionText("InvalidMimeType.Error", "{0} is invalid.", ext));
            });
        }

        public JsonResult<List<CoreModels.Theme>> GetInstalledThemes()
        {
            return API.Execute<List<CoreModels.Theme>>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                r.Data = CoreServices.UI.GetThemes();
            });
        }

        public JsonResult<List<CoreModels.Theme>> InstallTheme(CoreModels.Theme theme)
        {
            return API.Execute<List<CoreModels.Theme>>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                CoreServices.UI.InstallTheme(theme);
                r.Data = CoreServices.UI.GetThemes();
            });
        }

        public JsonResult<List<CoreModels.Theme>> UninstallTheme(string name)
        {
            return API.Execute<List<CoreModels.Theme>>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                CoreServices.UI.UninstallTheme(name);
                r.Data = CoreServices.UI.GetThemes();
            });
        }

        public JsonResult<bool> SaveWidget(string templateId, string layoutName, CoreModels.Widget widget)
        {
            return API.Execute<bool>(r =>
            {
                CoreServices.Security.VerifyActivityAuthorized("Content", "Administration");

                //Unfortnuately, we cannot just save content.  Widgets are persisted on the template and their properties (Css, Style, etc.) may have changed.  We need to re-save the template  //widget.Manifest.GetContentProvider().Save(widget.ContentJson);
                var pageTemplate = CoreServices.Portal.GetPageTemplateById(templateId);
                var layoutTemplate = CoreServices.Portal.GetLayoutTemplate(CoreServices.Portal.CurrentPortalId, layoutName);

                //a widget will live on either the page or template, never both.  
                if (pageTemplate != null)
                {
                    if (ReplaceWidget(pageTemplate.Widgets, widget))
                        CoreServices.Portal.Save(pageTemplate);
                }               
                
                if (layoutTemplate != null)
                {
                    if (ReplaceWidget(layoutTemplate.Widgets, widget))
                        CoreServices.Portal.Save(layoutTemplate);
                }
                r.Data = true;
            });
        }

        public JsonResult<List<CoreModels.WebReference>> GetWebReferences()
        {
            return API.Execute<List<CoreModels.WebReference>>(r =>
            {
                r.Data = CoreServices.Web.GetWebReferences();
            });
        }

        public JsonResult<bool> SaveWebReference(CoreModels.WebReference webReference)
        {
            return API.Execute<bool>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                r.Data = !string.IsNullOrEmpty(CoreServices.Web.Save(webReference));
            });
        }

        public JsonResult<bool> DeleteWebReference(string id)
        {
            return API.Execute<bool>(r =>
            {
                Security.VerifyActivityAuthorized("Portal", "Administration");
                r.Data = CoreServices.Web.DeleteWebReference(id);
            });
        }

        //todo: shouldn't there be a nice LINQ statement to accomplish this?
        private bool ReplaceWidget(List<CoreModels.Widget> widgets, CoreModels.Widget widget)
        {
            for (var i = 0; i < widgets.Count; i++)
            {
                if (widgets[i].Id == widget.Id)
                {
                    widgets[i] = widget;
                    return true;
                }
            }
            return false;
        }


    }
}
