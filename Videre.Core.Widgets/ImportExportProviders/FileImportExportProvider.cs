using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Models;
using CodeEndeavors.Extensions;
using System.Collections.Concurrent;
using Videre.Core.ImportExportProviders;

namespace Videre.Core.Widgets.ImportExportProviders
{
    public class FileImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "File"; } }

        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.File.Get(portalId).Select(f =>
                new ImportExportContent()
                {
                    Id = f.Id,
                    Name = f.Url,
                    Included = (export != null && export.Files != null ? export.Files.Exists(f2 => f2.Id == f.Id) : false),
                    Preview = string.Format("<img src=\"{0}\" class=\"img-polaroid\" style=\"height: 30px;\" />", f.MimeType.IndexOf("image/") > -1 ? Services.Portal.ResolveUrl("~/Core/f/" + f.Url) : Services.Portal.ResolveUrl("~/content/images/document.png"))
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

    }
}
