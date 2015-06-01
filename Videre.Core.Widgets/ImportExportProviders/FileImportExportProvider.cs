using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Models;
using CodeEndeavors.Extensions;
using System.Collections.Concurrent;
using Videre.Core.ImportExportProviders;
using Videre.Core.Services;

namespace Videre.Core.Widgets.ImportExportProviders
{
    public class FileImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "File"; } }
        public List<string> ProviderDependencies
        {
            get { return new List<string>(); }
        }

        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.File.Get(portalId).Select(f =>
                new ImportExportContent()
                {
                    Id = f.Id,
                    Name = f.Url,
                    Type = Name,
                    Included = (export != null && export.Files != null ? export.Files.Exists(f2 => f2.Id == f.Id) : false),
                    Preview = string.Format("<img src=\"{0}\" class=\"img-polaroid\" style=\"height: 30px;\" />", f.MimeType.IndexOf("image/") > -1 ? Services.Portal.ResolveUrl("~/Core/f/" + f.RenderUrl) : Services.Portal.ResolveUrl("~/content/images/document.png"))
                }).ToList();
        }

        public PortalExport Export(string id, PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            export = export ?? Services.ImportExport.GetPortalExport(portalId);

            var file = Services.File.GetById(id);
            if (file != null)
            {
                export.Files = export.Files != null ? export.Files : new List<Models.File>();
                export.Files.Add(file);
                export.FileContent = export.FileContent != null ? export.FileContent : new Dictionary<string, string>();
                export.FileContent[file.Id] = Services.Portal.GetFile(file.Id).GetFileBase64();
            }
            return export;
        }

        public void Import(PortalExport export, Dictionary<string, string> idMap, string portalId)
        {
            if (export.Files != null)
            {
                Logging.Logger.DebugFormat("Importing {0} files...", export.Files.Count);

                //todo:  embed file base64???
                foreach (var file in export.Files)
                {
                    var origId = file.Id;
                    ImportExport.SetIdMap<Models.File>(file.Id, Import(portalId, file), idMap);
                    if (export.FileContent.ContainsKey(origId))
                    {
                        var fileName = Services.Portal.GetFile(file.Id);
                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);
                        export.FileContent[origId].Base64ToFile(fileName);
                    }
                }
            }
        }

        private string Import(string portalId, Models.File file, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Services.File.Get(portalId, file.Url);
            file.PortalId = portalId;
            file.Id = existing != null ? existing.Id : null;
            return Services.File.Save(file, userId);
        }

    }
}
