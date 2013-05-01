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
    public class PageTemplateImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Page Template"; } }

        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Portal.GetPageTemplates(portalId).Select(t =>
                new ImportExportContent()
                {
                    Id = t.Id,
                    Name = t.Urls.Count > 0 ? t.Urls[0] : "Default",
                    Included = (export != null && export.PageTemplates != null ? export.PageTemplates.Exists(t2 => t2.Id == t.Id) : false)
                    //Preview = t.ToJson()
                }).ToList();
        }

        public PortalExport Export(string templateId, PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            export = export ?? Services.ImportExport.GetPortalExport(portalId);

            var pageTemplate = Services.Portal.GetPageTemplateById(templateId);
            if (pageTemplate != null)
            {
                var allWidgets = pageTemplate.Widgets.ToList();
                allWidgets.AddRange(pageTemplate.Layout.Widgets);

                var allRoleNames = pageTemplate.Roles;
                allRoleNames.AddRange(pageTemplate.Widgets.SelectMany(w => w.Roles));

                export.Roles = export.Roles ?? new List<Models.Role>();
                export.Roles.AddRange(Services.Account.GetRoles(portalId).Where(r => allRoleNames.Contains(r.Name)));

                export.Manifests = export.Manifests ?? new List<Models.WidgetManifest>();
                export.Manifests.AddRange(Services.Widget.GetWidgetManifests().Where(m => allWidgets.Select(w => w.ManifestId).Contains(m.Id)));

                export.PageTemplates = export.PageTemplates ?? new List<Models.PageTemplate>();
                export.PageTemplates.Add(pageTemplate);

                export.LayoutTemplates = export.LayoutTemplates ?? new List<Models.LayoutTemplate>();
                export.LayoutTemplates.Add(pageTemplate.Layout);

                export.WidgetContent = export.WidgetContent ?? new Dictionary<string, string>();
                export.WidgetContent.Merge(allWidgets.Where(w => w.Manifest.GetContentProvider() != null).ToDictionary(w => w.Id, wc => wc.GetContentJson()));
            }
            return export;
        }

    }
}
