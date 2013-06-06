using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Models;
using CodeEndeavors.Extensions;
using System.Collections.Concurrent;
using Videre.Core.ImportExportProviders;

namespace Videre.Core.Services
{
    public class ImportExport
    {
        //private static ConcurrentDictionary<string, Type> _importExportProviderTypes = new ConcurrentDictionary<string, Type>();
        private static ConcurrentDictionary<string, IImportExportProvider> _importExportProviders = new ConcurrentDictionary<string, IImportExportProvider>();

        public static void RegisterProvider(IImportExportProvider provider)
        {
            _importExportProviders[provider.Name] = provider;
        }

        public static IImportExportProvider GetProvider(string name)
        {
            if (_importExportProviders.ContainsKey(name))
                return _importExportProviders[name];

            throw new Exception(string.Format("Import Export Provider Type {0} not registered", name));
        }

        public static List<string> GetRegisteredProviders()
        {
            return _importExportProviders.Values.Select(p => p.Name).OrderBy(p => p).ToList();
        }

        //public static PortalExport ExportPortal(string portalId, bool includeFileContent = true)
        //{
        //    portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;

        //    var export = GetPortalExport(portalId);
        //    ExportAccount(portalId, export);
        //    ExportSecureActivities(portalId, export);
        //    ExportLocalizations(portalId, export);
        //    ExportTemplates(portalId, includeFileContent, export);
        //    return export;
        //}

        //public static PortalExport ExportTemplates(string portalId, bool includeFileContent, PortalExport export = null)
        //{
        //    export = export ?? GetPortalExport(portalId);
        //    if (export.Roles == null)
        //        export.Roles = Account.GetRoles(portalId); //we need mapping of roles!

        //    export.Manifests = Widget.GetWidgetManifests();
        //    export.Files = File.Get(portalId); //todo:  somehow export file?!?!
        //    export.PageTemplates = Portal.GetPageTemplates(portalId);

        //    export.LayoutTemplates = Portal.GetLayoutTemplates(portalId);

        //    //grab all widgets that have a content provider and create dictionary of <WidgetId, ContentJson>
        //    //TODO:  guard against no widgets with content???!?
        //    var allWidgets = new List<Models.Widget>();
        //    allWidgets.AddRange(export.PageTemplates.SelectMany(w => w.Widgets).Where(w => w.Manifest.GetContentProvider() != null));
        //    allWidgets.AddRange(export.LayoutTemplates.SelectMany(w => w.Widgets).Where(w => w.Manifest.GetContentProvider() != null));
        //    allWidgets = allWidgets.Distinct(w => w.Id).ToList();
        //    //since content can be shared between widgets, we only want to store it once!
        //    export.WidgetContent = allWidgets.ToDictionary(w => w.Id, wc => wc.GetContentJson());

        //    if (includeFileContent)
        //    {
        //        export.FileContent = new Dictionary<string, string>();
        //        foreach (var file in export.Files)
        //        {
        //            export.FileContent[file.Id] = Portal.GetFile(file.Id).GetFileBase64();
        //        }
        //    }
        //    return export;
        //}

        //public static PortalExport ExportSecureActivities(string portalId, PortalExport export = null)
        //{
        //    export = export ?? GetPortalExport(portalId);
        //    if (export.SecureActivities == null)
        //        export.SecureActivities = Security.GetSecureActivities(portalId: portalId); //we need mapping of roles!
        //    return export;
        //}

        //public static PortalExport ExportAccount(string portalId, PortalExport export = null)
        //{
        //    export = export ?? GetPortalExport(portalId);
        //    export.Roles = Account.GetRoles(portalId);
        //    export.Users = Account.GetUsers(portalId);
        //    export.SecureActivities = Security.GetSecureActivities(portalId: portalId);
        //    return export;
        //}

        //public static PortalExport ExportLocalizations(string portalId, PortalExport export = null)
        //{
        //    export = export ?? GetPortalExport(portalId);
        //    export.Localizations = Localization.Get(portalId).Where(l => l.Type != LocalizationType.WidgetContent).ToList();
        //    //don't include widget content
        //    return export;
        //}

        public static bool Import(PortalExport export, string portalId)
        {
            var idMap = new Dictionary<string, string>();

            var portal = Portal.GetPortalById(portalId);
            if (portal != null)
            {
                export.Portal.Id = portal.Id;
                export.Portal.Name = portal.Name;
                //IMPORTANT:  since we pass in the portalId that we want to import into, we need to ensure that is used, therefore, the name must match as that is how the duplicate lookup is done.  (i.e. {Id: 1, Name=Default} {Id: 2, Name=Foo}.  If we import Id:2 and its name importing is Default)
                export.Portal.Default = portal.Default;
            }
            else
                throw new Exception("Portal Not found: " + portalId);

            Portal.Save(export.Portal);
            portal = Portal.GetPortal(export.Portal.Name);
            SetIdMap<Models.Portal>(portal.Id, export.Portal.Id, idMap);

            if (export.Roles != null)
            {
                Logging.Logger.DebugFormat("Importing {0} roles...", export.Roles.Count);
                foreach (var role in export.Roles)
                    SetIdMap<Role>(role.Id, Account.ImportRole(portal.Id, role), idMap);
            }
            if (export.Users != null)
            {
                Logging.Logger.DebugFormat("Importing {0} users...", export.Users.Count);
                foreach (var exportUser in export.Users)
                    SetIdMap<User>(exportUser.Id, Account.ImportUser(portal.Id, exportUser, idMap), idMap);
            }

            if (export.SecureActivities != null)
            {
                Logging.Logger.DebugFormat("Importing {0} secure activities...", export.SecureActivities.Count);
                foreach (var exportActivity in export.SecureActivities)
                    SetIdMap<SecureActivity>(exportActivity.Id, Security.Import(portal.Id, exportActivity, idMap),
                        idMap);
            }

            if (export.Files != null)
            {
                Logging.Logger.DebugFormat("Importing {0} files...", export.Files.Count);

                //todo:  embed file base64???
                foreach (var file in export.Files)
                {
                    var origId = file.Id;
                    SetIdMap<Models.File>(file.Id, File.Import(portal.Id, file), idMap);
                    if (export.FileContent.ContainsKey(origId))
                    {
                        var fileName = Portal.GetFile(file.Id);
                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);
                        export.FileContent[origId].Base64ToFile(fileName);
                    }
                }
            }

            if (export.Manifests != null)
            {
                Logging.Logger.DebugFormat("Importing {0} manifests...", export.Manifests.Count);
                foreach (var manifest in export.Manifests)
                    SetIdMap<WidgetManifest>(manifest.Id, Widget.Import(manifest), idMap);
            }
            if (export.Localizations != null)
            {
                Logging.Logger.DebugFormat("Importing {0} localizations...", export.Localizations.Count);
                foreach (var exportLocalization in export.Localizations)
                    SetIdMap<Models.Localization>(exportLocalization.Id, Localization.Import(portal.Id, exportLocalization), idMap);
            }
            if (export.PageTemplates != null)
            {
                Logging.Logger.DebugFormat("Importing {0} page templates...", export.PageTemplates.Count);
                foreach (var exportTemplate in export.PageTemplates)
                    SetIdMap<PageTemplate>(exportTemplate.Id, Import(portal.Id, exportTemplate, export.WidgetContent, idMap), idMap);
            }
            if (export.LayoutTemplates != null)
            {
                Logging.Logger.DebugFormat("Importing {0} layout templates...", export.LayoutTemplates.Count);
                foreach (var exportTemplate in export.LayoutTemplates)
                    SetIdMap<LayoutTemplate>(exportTemplate.Id, Import(portal.Id, exportTemplate, export.WidgetContent, idMap), idMap);
            }
            if (export.WebReferences != null)
            {
                Logging.Logger.DebugFormat("Importing {0} web references...", export.WebReferences.Count);
                foreach (var exportWebReference in export.WebReferences)
                    SetIdMap<Models.WebReference>(exportWebReference.Id, Web.Import(portal.Id, exportWebReference, idMap), idMap);
            }

            return true;
        }

        public static string Import(string portalId, LayoutTemplate template, Dictionary<string, string> widgetContent, Dictionary<string, string> idMap, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Portal.GetLayoutTemplate(portalId, template.LayoutName);
            template.PortalId = portalId;
            template.Roles = Security.GetNewRoleIds(template.Roles, idMap);
            template.Id = existing != null ? existing.Id : null;
            foreach (var widget in template.Widgets)
            {
                widget.ManifestId = GetIdMap<WidgetManifest>(widget.ManifestId, idMap);
                widget.Roles = Security.GetNewRoleIds(widget.Roles, idMap);

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
            return Portal.Save(template);
        }

        public static string Import(string portalId, PageTemplate pageTemplate, Dictionary<string, string> widgetContent, Dictionary<string, string> idMap, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Portal.GetPageTemplate(pageTemplate.Urls.Count > 0 ? pageTemplate.Urls[0] : "", portalId);
            //todo:  by first url ok???
            pageTemplate.PortalId = portalId;
            pageTemplate.Roles = Security.GetNewRoleIds(pageTemplate.Roles, idMap);
            pageTemplate.Id = existing != null ? existing.Id : null;

            foreach (var widget in pageTemplate.Widgets)
            {
                widget.ManifestId = GetIdMap<WidgetManifest>(widget.ManifestId, idMap);
                widget.Roles = Security.GetNewRoleIds(widget.Roles, idMap);

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
            return Portal.Save(pageTemplate);
        }

        private static string SetIdMap<T>(string id, string value, Dictionary<string, string> map)
        {
            var key = string.Format("{0}~{1}", typeof(T), id);
            return map[key] = value;
        }

        public static string GetIdMap<T>(string id, Dictionary<string, string> map)
        {
            var key = string.Format("{0}~{1}", typeof(T), id);
            return map.GetSetting<string>(key, null);
        }

        public static PortalExport GetPortalExport(string portalId)
        {
            var export = new PortalExport();
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            export.Portal = Portal.GetPortalById(portalId);
            return export;
        }

    }
}
