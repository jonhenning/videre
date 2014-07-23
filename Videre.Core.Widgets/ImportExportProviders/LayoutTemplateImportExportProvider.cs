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
    public class LayoutTemplateImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Layout Template"; } }
        public List<string> ProviderDependencies
        {
            get { return new List<string>() { "Role", "Widget Manifest" }; }
        }

        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Portal.GetLayoutTemplates(portalId).Select(t =>
                new ImportExportContent()
                {
                    Id = t.Id,
                    Name = t.LayoutName,
                    Type = Name,
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

                var allRoleNames = layoutTemplate.RoleIds;
                allRoleNames.AddRange(layoutTemplate.Widgets.SelectMany(w => w.RoleIds));

                export.Roles = export.Roles ?? new List<Models.Role>();
                export.Roles.AddRange(Services.Account.GetRoles(portalId).Where(r => allRoleNames.Contains(r.Name)));

                export.Manifests = export.Manifests ?? new List<Models.WidgetManifest>();
                export.Manifests.AddRange(Services.Widget.GetWidgetManifests().Where(m => allWidgets.Select(w => w.ManifestId).Contains(m.Id)));

                export.LayoutTemplates = export.LayoutTemplates ?? new List<Models.LayoutTemplate>();
                export.LayoutTemplates.Add(layoutTemplate);

                export.WidgetContent = export.WidgetContent ?? new Dictionary<string, string>();
                //todo: should fix extension method to use dictionary, not create a new one!
                export.WidgetContent = export.WidgetContent.Merge(allWidgets.Where(w => w.Manifest.GetContentProvider() != null).ToDictionary(w => w.Id, wc => wc.GetContentJson("db")));
            }
            return export;
        }

        public void Import(PortalExport export, Dictionary<string, string> idMap, string portalId)
        {
            if (export.LayoutTemplates != null)
            {
                Logging.Logger.DebugFormat("Importing {0} layout templates...", export.LayoutTemplates.Count);
                foreach (var exportTemplate in export.LayoutTemplates)
                    ImportExport.SetIdMap<LayoutTemplate>(exportTemplate.Id, Import(portalId, exportTemplate, export.WidgetContent, idMap), idMap);
            }
        }

        private string Import(string portalId, LayoutTemplate template, Dictionary<string, string> widgetContent, Dictionary<string, string> idMap, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Services.Portal.GetLayoutTemplate(portalId, template.LayoutName);
            template.PortalId = portalId;
            template.RoleIds = Security.GetNewRoleIds(template.RoleIds, idMap);
            template.Id = existing != null ? existing.Id : null;
            foreach (var widget in template.Widgets)
            {
                widget.ManifestId = ImportExport.GetIdMap<WidgetManifest>(widget.ManifestId, idMap);
                widget.RoleIds = Security.GetNewRoleIds(widget.RoleIds, idMap);

                //todo: not creating/mapping new widget ids?
                if (widgetContent.ContainsKey(widget.Id))
                {
                    var contentProvider = widget.Manifest.GetContentProvider();
                    if (contentProvider != null)
                        widget.ContentIds =
                            contentProvider.Import(portalId, widget.Id, widgetContent[widget.Id], idMap).Values.ToList();
                    //returns mapped dictionary of old id to new id... we just need to use the new ids
                }
            }
            return Services.Portal.Save(template);
        }

    }
}
