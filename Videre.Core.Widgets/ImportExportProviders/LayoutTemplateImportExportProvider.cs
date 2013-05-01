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
    public class LayoutTemplateImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Layout Template"; } }

        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Portal.GetLayoutTemplates(portalId).Select(t =>
                new ImportExportContent()
                {
                    Id = t.Id,
                    Name = t.LayoutName,
                    Included = (export != null && export.LayoutTemplates != null ? export.LayoutTemplates.Exists(t2 => t2.Id == t.Id) : false)
                    //Preview = t.ToJson()
                }).ToList();
        }

        public PortalExport Export(string templateId, PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            export = export ?? Services.ImportExport.GetPortalExport(portalId);

            var layoutTemplate = Services.Portal.GetLayoutTemplateById(templateId);
            if (layoutTemplate != null)
            {
                var allWidgets = layoutTemplate.Widgets.ToList();

                var allRoleNames = layoutTemplate.Roles;
                allRoleNames.AddRange(layoutTemplate.Widgets.SelectMany(w => w.Roles));

                export.Roles = export.Roles ?? new List<Models.Role>();
                export.Roles.AddRange(Services.Account.GetRoles(portalId).Where(r => allRoleNames.Contains(r.Name)));

                export.Manifests = export.Manifests ?? new List<Models.WidgetManifest>();
                export.Manifests.AddRange(Services.Widget.GetWidgetManifests().Where(m => allWidgets.Select(w => w.ManifestId).Contains(m.Id)));

                export.LayoutTemplates = export.LayoutTemplates ?? new List<Models.LayoutTemplate>();
                export.LayoutTemplates.Add(layoutTemplate);

                export.WidgetContent = export.WidgetContent ?? new Dictionary<string, string>();
                export.WidgetContent.Merge(allWidgets.Where(w => w.Manifest.GetContentProvider() != null).ToDictionary(w => w.Id, wc => wc.GetContentJson()));
            }
            return export;
        }

    }
}
