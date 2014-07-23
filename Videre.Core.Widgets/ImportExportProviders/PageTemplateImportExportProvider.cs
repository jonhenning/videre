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
    public class PageTemplateImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Page Template"; } }
        public List<string> ProviderDependencies
        {
            get { return new List<string>() {"Role", "Widget Manifest", "Layout Template"}; }
        }

        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Portal.GetPageTemplates(portalId).Select(t =>
                new ImportExportContent()
                {
                    Id = t.Id,
                    Name = t.Urls.Count > 0 ? t.Urls[0] : "Default",
                    Type = Name,
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
                export.LayoutTemplates = export.LayoutTemplates ?? new List<Models.LayoutTemplate>();
                if (!export.LayoutTemplates.Exists(l => l.Id == pageTemplate.LayoutId)) //if we haven't already added this layout, then we will need its widget (manifests and content)
                    allWidgets.AddRange(pageTemplate.Layout.Widgets);   

                var allRoleIds = pageTemplate.RoleIds;
                allRoleIds.AddRange(pageTemplate.Widgets.SelectMany(w => w.RoleIds));

                export.Roles = export.Roles ?? new List<Models.Role>();
                export.Roles.AddRange(Services.Account.GetRoles(portalId).Where(r => allRoleIds.Contains(r.Id)));

                export.Manifests = export.Manifests ?? new List<Models.WidgetManifest>();
                export.Manifests.AddRange(Services.Widget.GetWidgetManifests().Where(m => allWidgets.Select(w => w.ManifestId).Contains(m.Id)));

                export.PageTemplates = export.PageTemplates ?? new List<Models.PageTemplate>();
                export.PageTemplates.Add(pageTemplate);

                if (!export.LayoutTemplates.Exists(l => l.Id == pageTemplate.LayoutId))
                    export.LayoutTemplates.Add(pageTemplate.Layout);

                export.WidgetContent = export.WidgetContent ?? new Dictionary<string, string>();
                export.WidgetContent = export.WidgetContent.Merge(allWidgets.Where(w => w.Manifest.GetContentProvider() != null).ToDictionary(w => w.Id, wc => wc.GetContentJson("db")));
            }
            return export;
        }

        public void Import(PortalExport export, Dictionary<string, string> idMap, string portalId)
        {
            if (export.PageTemplates != null)
            {
                Logging.Logger.DebugFormat("Importing {0} page templates...", export.PageTemplates.Count);
                foreach (var exportTemplate in export.PageTemplates)
                    ImportExport.SetIdMap<PageTemplate>(exportTemplate.Id, Import(portalId, exportTemplate, export.WidgetContent, idMap), idMap);
            }
        }

        private string Import(string portalId, PageTemplate pageTemplate, Dictionary<string, string> widgetContent, Dictionary<string, string> idMap, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Services.Portal.GetPageTemplate(pageTemplate.Urls.Count > 0 ? pageTemplate.Urls[0] : "", portalId);
            //todo:  by first url ok???
            pageTemplate.PortalId = portalId;
            pageTemplate.RoleIds = Security.GetNewRoleIds(pageTemplate.RoleIds, idMap);
            pageTemplate.Id = existing != null ? existing.Id : null;
            pageTemplate.LayoutId = ImportExport.GetIdMap<LayoutTemplate>(pageTemplate.LayoutId, idMap);

            //conversion for older export packages to lookup layoutIds
            if (string.IsNullOrEmpty(pageTemplate.LayoutId))
            {
                var layoutTemplate = Services.Portal.GetLayoutTemplate(portalId, pageTemplate.LayoutName);
                if (layoutTemplate != null)
                    pageTemplate.LayoutId = layoutTemplate.Id;
            }


            foreach (var widget in pageTemplate.Widgets)
            {
                widget.ManifestId = ImportExport.GetIdMap<WidgetManifest>(widget.ManifestId, idMap);
                widget.RoleIds = Security.GetNewRoleIds(widget.RoleIds, idMap);

                //todo: not creating/mapping new widget ids?
                if (widgetContent.ContainsKey(widget.Id))
                {
                    var contentProvider = widget.Manifest.GetContentProvider();
                    if (contentProvider != null)
                        widget.ContentIds = contentProvider.Import(portalId, widget.Id, widgetContent[widget.Id], idMap).Values.ToList();
                    //returns mapped dictionary of old id to new id... we just need to use the new ids
                }
            }
            Logging.Logger.DebugFormat("Importing page template {0}", pageTemplate.ToJson());
            return Services.Portal.Save(pageTemplate);
        }


    }
}
