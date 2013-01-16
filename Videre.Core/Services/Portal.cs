using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using CodeEndeavors.Extensions;
using CodeEndeavors.ResourceManager.Extensions;
using Videre.Core.Models;
using DomainObjects = CodeEndeavors.ResourceManager.DomainObjects;
using CodeEndeavors.Cache;

namespace Videre.Core.Services
{
    public class Portal
    {
        public static bool IsInRequest
        {
            get
            {
                try
                {
                    return HttpContext.Current != null && HttpContext.Current.Request != null;
                }
                catch (Exception ex)    //todo: why would this occur?
                {
                    return false;
                }
            }
        }

        //wrapping these calls to potentially enforce prefixing of keys... 
        public static T GetRequestContextData<T>(string key, T defaultValue)
        {
            if (IsInRequest) //todo: allow this to be on thread?
                return System.Web.HttpContext.Current.Items.GetSetting<T>(key, defaultValue);
            return defaultValue; // default(T);
        }
        public static void SetRequestContextData<T>(string key, T value)
        {
            if (IsInRequest) //todo: allow this to be on thread?
                System.Web.HttpContext.Current.Items[key] = value;
        }

        public static Models.PageTemplate CurrentTemplate  //todo: force ViewBag use of use HttpContext?
        {
            get
            {
                return GetRequestContextData<Models.PageTemplate>("CurrentTemplate", null);
            }
            set
            {
                SetRequestContextData("CurrentTemplate", value);
            }
        }
        public static string CurrentUrl
        {
            get
            {
                return GetRequestContextData("CurrentUrl", "");
            }
            set
            {
                SetRequestContextData("CurrentUrl", value);
            }
        }
        public static Dictionary<string, string> CurrentUrlMatchedGroups
        {
            get
            {
                return GetRequestContextData("CurrentUrlMatchedGroups", new Dictionary<string, string>());
            }
            set
            {
                SetRequestContextData("CurrentUrlMatchedGroups", value);
            }
        }
        public static T GetCurrentUrlSegment<T>(string name, T defaultValue, bool throwError = false)
        {
            if (!Portal.CurrentUrlMatchedGroups.ContainsKey(name) && throwError)
                throw new Exception(string.Format(Localization.GetLocalization(LocalizationType.Exception, "UrlGroupNotFound.Error", "The url for this template was expected to have a segment with {{{0}:datatype}} in it", "Core"), name));

            return CurrentUrlMatchedGroups.GetSetting<T>(name, defaultValue);
        }

        public static Models.PageTemplate GetPageTemplateFromUrl(string url, string portalId = null)
        {
            var temp = Portal.GetWidgetManifests(); //ensure manifests are loaded

            portalId = string.IsNullOrEmpty(portalId) ? CurrentPortalId : portalId;
            //todo:  use widgets to determine authentication...
            //if (!Services.Account.IsAuthenticated)
            //    Url = "Account/LogOn";

            var template = GetMatchedTemplate(url, portalId);
            if (template == null)
                template = GetMatchedTemplate("", portalId);

            //if (template != null)
            //{
            //    var regExUrl = template.Urls.Where(u => u.StartsWith("regex:") && Regex.Match(url, u.Replace("regex:", ""), RegexOptions.IgnoreCase).Success).SingleOrDefault();
            //    if (regExUrl != null)
            //    {
            //        var match = Regex.Match(url, regExUrl.Replace("regex:", ""), RegexOptions.IgnoreCase);
            //        if (match.Groups.Count > 1 && match.Groups[1].Captures.Count > 0)
            //            template.UrlId = match.Groups[1].Captures[0].Value;
            //    }
            //}

            return template;
        }
        public static List<Models.PageTemplate> GetPageTemplates(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? CurrentPortalId : portalId;
            return Repository.Current.GetResources<Models.PageTemplate>("Template", t => t.Data.PortalId == portalId, false).Select(t => t.Data).ToList();
        }
        public static Models.PageTemplate GetPageTemplateById(string id)
        {
            var res = Repository.Current.GetResourceById<Models.PageTemplate>(id);
            if (res != null)
                return res.Data;
            return null;
        }
        public static Models.PageTemplate GetPageTemplate(string url, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? CurrentPortalId : portalId;
            var templates = GetPageTemplates(portalId);
            return templates.Where(t2 => (string.IsNullOrEmpty(url) && t2.Urls.Count == 0) || t2.Urls.Contains(url) && t2.PortalId == portalId).SingleOrDefault();
        }

        public static Models.PageTemplate GetMatchedTemplate(string url, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? CurrentPortalId : portalId;

            var templates = Repository.Current.GetResources<Models.PageTemplate>("Template", t => t.Data.PortalId == portalId).Select(t => t.Data);
            if (!string.IsNullOrEmpty(url))
            {
                var bestMatch = GetBestMatchedUrl(url, templates.SelectMany(t => t.Urls));
                if (!string.IsNullOrEmpty(bestMatch))
                {
                //var urls = templates.SelectMany(t => t.Urls);
                //var queries = new List<DomainObjects.Query<string>>() {
                //    new DomainObjects.Query<string>(u => Services.RouteParser.Parse(u, url).Keys.Count > 0, 1)};

                //var matchedUrls = queries.GetMatches(urls, false);
                //if (matchedUrls.Count > 0)
                //{
                //    //our most specific match is determined by number of matching groups from regex... - if tie then use length of matchedUrl
                //    var bestMatch = (from u in matchedUrls
                //                     orderby Services.RouteParser.Parse(u, url).Keys.Count descending, u.Length descending
                //                     select u).FirstOrDefault();

                    if (IsInRequest)    //todo: only need this when in request, otherwise ignore... perhaps should be moved out to controller...
                    {
                        Portal.CurrentUrlMatchedGroups = Services.RouteParser.Parse(bestMatch, url);
                        CurrentUrl = Portal.CurrentUrlMatchedGroups["0"];
                    }
                    return templates.Where(t => t.Urls.Contains(bestMatch)).SingleOrDefault();
                }
            }
            CurrentUrl = "";
            return templates.Where(t => t.Urls.Count == 0).FirstOrDefault();  //grab default template
        }

        public static string GetBestMatchedUrl(string url, IEnumerable<string> urls)
        {
            var queries = new List<DomainObjects.Query<string>>() { new DomainObjects.Query<string>(u => Services.RouteParser.Parse(u, url).Keys.Count > 0, 1) };

            var matchedUrls = queries.GetMatches(urls, false);
            if (matchedUrls.Count > 0)
            {
                //our most specific match is determined by number of matching groups from regex... - if tie then use length of matchedUrl
                return (from u in matchedUrls
                        orderby Services.RouteParser.Parse(u, url).Keys.Count descending, u.Length descending
                        select u).FirstOrDefault();
            }
            return null;
        }

        public static string Import(string portalId, Models.PageTemplate pageTemplate, Dictionary<string, string> widgetContent, Dictionary<string, string> idMap, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Portal.GetPageTemplate(pageTemplate.Urls.Count > 0 ? pageTemplate.Urls[0] : "", portalId);   //todo:  by first url ok???
            pageTemplate.PortalId = portalId;
            pageTemplate.Roles = Security.GetNewRoleIds(pageTemplate.Roles, idMap);
            pageTemplate.Id = existing != null ? existing.Id : null;

            foreach (var widget in pageTemplate.Widgets)
            {
                widget.ManifestId = GetIdMap<Models.WidgetManifest>(widget.ManifestId, idMap);
                widget.Roles = Security.GetNewRoleIds(widget.Roles, idMap);

                //todo: not creating/mapping new widget ids?
                if (widgetContent.ContainsKey(widget.Id))
                {
                    var contentProvider = widget.Manifest.GetContentProvider();
                    if (contentProvider != null)
                        widget.ContentIds = contentProvider.Import(portalId, widget.Id, widgetContent[widget.Id], idMap).Values.ToList(); //returns mapped dictionary of old id to new id... we just need to use the new ids
                }
            }
            Logging.Logger.DebugFormat("Importing page template {0}", pageTemplate.ToJson());
            return Portal.Save(pageTemplate);
        }

        //grab content ids that are used by more than one widget
        public static List<string> GetSharedContentIds()
        {
            var ids = GetPageTemplates().SelectMany(t => t.Widgets).SelectMany(w => w.ContentIds).ToList();
            ids.AddRange(GetLayoutTemplates().SelectMany(t => t.Widgets).SelectMany(w => w.ContentIds));
            return ids.GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        }

        public static string Save(Models.PageTemplate pageTemplate, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            pageTemplate.PortalId = string.IsNullOrEmpty(pageTemplate.PortalId) ? CurrentPortalId : pageTemplate.PortalId;
            //strip empty urls
            pageTemplate.Urls = pageTemplate.Urls.Where(u => !string.IsNullOrEmpty(u)).ToList();

            if (!IsDuplicate(pageTemplate))
            {
                var prevTemplate = GetPageTemplateById(pageTemplate.Id);

                if (prevTemplate != null)
                {
                    var missing = (from p in prevTemplate.Widgets
                                   where !(from w in pageTemplate.Widgets select w.Id).Contains(p.Id)
                                   select p);
                    foreach (var widget in missing)
                        widget.RemoveContent();
                }
                Repository.Current.StoreResource("Template", null, pageTemplate, userId);

                foreach (var widget in pageTemplate.Widgets)
                {
                    //widget.TemplateId = Template.Id;    //is this a hack?
                    if (widget.ContentJson != null)
                        widget.SaveContentJson(widget.ContentJson);
                }

                //after contentIds assigned, need to save them!
                var res = Repository.Current.StoreResource("Template", null, pageTemplate, userId);
                return res.Id;
            }
            else
                throw new Exception(string.Format(Localization.GetLocalization(LocalizationType.Exception, "DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "Core"), "Template"));
        }
        public static bool DeletePageTemplate(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var res = Repository.Current.GetResourceById<Models.PageTemplate>(id);
            if (res != null)
            {
                //foreach (var widget in res.Data.Widgets)
                //    widget.DeleteContent();
                Repository.Current.Delete(res);
            }
            return res != null;
        }
        public static bool Exists(Models.PageTemplate pageTemplate)
        {
            var templates = GetPageTemplates();
            if (pageTemplate.IsDefault)
                return templates.Exists(t => t.IsDefault);
            foreach (var url in pageTemplate.Urls)
            {
                //var t = GetTemplate(url, template.PortalId);
                var t = templates.Where(t2 => t2.Urls.Contains(url) && t2.PortalId == pageTemplate.PortalId).SingleOrDefault();
                if (t != null)
                    return true;
            }
            return false;
        }
        public static bool IsDuplicate(Models.PageTemplate pageTemplate)
        {
            var templates = GetPageTemplates();
            foreach (var url in pageTemplate.Urls)
            {
                //var t = GetTemplate(url, template.PortalId);
                var t = templates.Where(t2 => t2.Urls.Contains(url) && t2.PortalId == pageTemplate.PortalId).SingleOrDefault();
                if (t != null && t.Id != pageTemplate.Id)
                    return true;
            }
            return false;
        }

        public static List<Models.LayoutTemplate> GetLayoutTemplates(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? CurrentPortalId : portalId;
            return Repository.Current.GetResources<Models.LayoutTemplate>("LayoutTemplate", t => t.Data.PortalId == portalId, false).Select(t => t.Data).ToList();
        }
        public static Models.LayoutTemplate GetLayoutTemplateById(string id)
        {
            var res = Repository.Current.GetResourceById<Models.LayoutTemplate>(id);
            if (res != null)
                return res.Data;
            return null;
        }
        public static Models.LayoutTemplate GetLayoutTemplate(string portalId, string layoutName)
        {
            var template = Repository.Current.GetResources<Models.LayoutTemplate>("LayoutTemplate", t => t.Data.PortalId == portalId && t.Data.LayoutName == layoutName, true).SingleOrDefault();
            return template != null ? template.Data : null;
        }
        public static string Import(string portalId, Models.LayoutTemplate template, Dictionary<string, string> widgetContent, Dictionary<string, string> idMap, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Portal.GetLayoutTemplate(portalId, template.LayoutName);
            template.PortalId = portalId;
            template.Roles = Security.GetNewRoleIds(template.Roles, idMap);
            template.Id = existing != null ? existing.Id : null;
            foreach (var widget in template.Widgets)
            {
                widget.ManifestId = GetIdMap<Models.WidgetManifest>(widget.ManifestId, idMap);
                widget.Roles = Security.GetNewRoleIds(widget.Roles, idMap);

                //todo: not creating/mapping new widget ids?
                if (widgetContent.ContainsKey(widget.Id))
                {
                    var contentProvider = widget.Manifest.GetContentProvider();
                    if (contentProvider != null)
                        widget.ContentIds = contentProvider.Import(portalId, widget.Id, widgetContent[widget.Id], idMap).Values.ToList(); //returns mapped dictionary of old id to new id... we just need to use the new ids
                }
            }
            return Portal.Save(template);
        }
        public static string Save(Models.LayoutTemplate template, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            template.PortalId = string.IsNullOrEmpty(template.PortalId) ? CurrentPortalId : template.PortalId;
            if (!IsDuplicate(template))
            {
                var prevTemplate = GetPageTemplateById(template.Id);
                if (prevTemplate != null)
                {
                    var missing = (from p in prevTemplate.Widgets
                                   where !(from w in template.Widgets select w.Id).Contains(p.Id)
                                   select p);
                    foreach (var widget in missing)
                        widget.RemoveContent();
                }

                Repository.Current.StoreResource("LayoutTemplate", null, template, userId);
                foreach (var widget in template.Widgets)
                {
                    //widget.TemplateId = Template.Id;    //is this a hack?
                    if (widget.ContentJson != null) //needed for initial update where we want to assign ContentIds and not json...   may cause issue with blanking out contentids by trying to blank out contentjson... TODO:
                        widget.SaveContentJson(widget.ContentJson);
                }
                //after contentIds assigned, need to save them!
                var res = Repository.Current.StoreResource("LayoutTemplate", null, template, userId);
                return res.Id;

            }
            else
                throw new Exception(string.Format(Localization.GetLocalization(LocalizationType.Exception, "DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "Core"), "LayoutTemplate"));
        }
        public static bool DeleteLayoutTemplate(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var res = Repository.Current.GetResourceById<Models.PageTemplate>(id);
            if (res != null)
            {
                //foreach (var widget in res.Data.Widgets)
                //    widget.DeleteContent();
                Repository.Current.Delete(res);
            }
            return res != null;
        }
        public static bool Exists(Models.LayoutTemplate template)
        {
            return GetLayoutTemplate(template.PortalId, template.LayoutName) != null;
        }
        public static bool IsDuplicate(Models.LayoutTemplate template)
        {
            var t = GetLayoutTemplate(template.PortalId, template.LayoutName);
            return t != null && t.Id != template.Id;
        }

        public static List<Models.WidgetManifest> GetWidgetManifests()
        {
            return Repository.Current.GetResources<Models.WidgetManifest>("WidgetManifest").Select(m => m.Data).OrderBy(i => i.Name).ToList();
        }
        public static Models.WidgetManifest GetWidgetManifest(string fullName)
        {
            return GetWidgetManifests().Where(m => m.FullName == fullName).FirstOrDefault();
        }
        public static Models.WidgetManifest GetWidgetManifestById(string Id)
        {
            return GetWidgetManifests().Where(m => m.Id == Id).FirstOrDefault();
        }
        public static string Import(Models.WidgetManifest manifest, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = GetWidgetManifest(manifest.FullName);
            manifest.Id = existing != null ? existing.Id : null;
            return Save(manifest, userId);
        }
        public static string Save(Models.WidgetManifest manifest, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            if (!IsDuplicate(manifest))
            {
                var res = Repository.Current.StoreResource("WidgetManifest", null, manifest, userId);
                return res.Id;
            }
            else
                throw new Exception(string.Format(Localization.GetLocalization(LocalizationType.Exception, "DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "Core"), "Widget Manifest"));
        }
        public static bool IsDuplicate(Models.WidgetManifest manifest)
        {
            var m = GetWidgetManifest(manifest.FullName);
            if (m != null)
                return m.Id != manifest.Id;
            return false;
        }
        public static bool Exists(Models.WidgetManifest manifest)
        {
            var m = GetWidgetManifest(manifest.FullName);
            return (m != null);
        }
        public static bool DeleteManifest(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var res = Repository.Current.GetResourceById<Models.WidgetManifest>(id);
            if (res != null)
            {
                // remove from all templates first!
                var pageTemplates = GetPageTemplates().Where(t => t.Widgets.Exists(w => w.ManifestId == id)).ToList();
                pageTemplates.ForEach(t =>
                {
                    t.Widgets.RemoveAll(w => w.ManifestId == id);
                    Save(t);
                });

                var layoutTemplates = GetLayoutTemplates().Where(t => t.Widgets.Exists(w => w.ManifestId == id)).ToList();
                layoutTemplates.ForEach(t =>
                {
                    t.Widgets.RemoveAll(w => w.ManifestId == id);
                    Save(t);
                });

                Repository.Current.Delete(res);
            }
            return res != null;
        }

        public static string CurrentPortalId
        {
            get
            {
                var portal = CurrentPortal;
                return portal != null ? portal.Id : null;
            }
        }
        public static Models.Portal CurrentPortal
        {
            get
            {
                //TODO: cache this beyond current request
                return CacheState.PullRequestCache<Models.Portal>("CurrentPortal", delegate()
                {
                    var portals = GetPortals();
                    var bestMatch = GetBestMatchedUrl(RequestRootUrl, portals.SelectMany(t => t.Aliases));
                    Models.Portal portal = null;
                    if (!string.IsNullOrEmpty(bestMatch))
                        portal = portals.Where(t => t.Aliases.Contains(bestMatch)).FirstOrDefault();
                    return portal != null ? portal : GetDefaultPortal();
                });
            }
        }
        public static List<Models.Portal> GetPortals()
        {
            return Repository.Current.GetResources<Models.Portal>("Portal").Select(m => m.Data).ToList();
        }
        public static Models.Portal GetPortalById(string id)
        {
            return GetPortals().Where(m => m.Id == id).FirstOrDefault();
        }
        public static Models.Portal GetPortal(string name)
        {
            return GetPortals().Where(m => m.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
        public static Models.Portal GetDefaultPortal()
        {
            return GetPortals().Where(m => m.Default).FirstOrDefault();
        }
        public static bool Save(Models.Portal portal, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            if (!IsDuplicate(portal))
            {
                var portals = Portal.GetPortals();
                if (portal.Default)
                {
                    foreach (var p in portals.Where(p => p.Default))
                    {
                        p.Default = false;
                        Repository.Current.StoreResource("Portal", null, p, userId);
                    }
                }
                else if (!portals.Exists(p => p.Default && p.Id != portal.Id))
                    throw new Exception(string.Format(Localization.GetLocalization(LocalizationType.Exception, "DefaultPortalRequired.Error", "At least one portal must be marked as the default.", "Core"), "Portal"));

                Repository.Current.StoreResource("Portal", null, portal, userId);
            }
            else
                throw new Exception(string.Format(Localization.GetLocalization(LocalizationType.Exception, "DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "Core"), "Portal"));
            return true;
        }
        public static bool IsDuplicate(Models.Portal portal)
        {
            var p = GetPortal(portal.Name);
            if (p != null)
                return p.Id != portal.Id;
            return false;
        }
        public static bool Exists(Models.Portal portal)
        {
            return GetPortal(portal.Name) != null;
        }
        public static bool DeletePortal(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var res = Repository.Current.GetResourceById<Models.Portal>(id);
            if (res != null)
                Repository.Current.Delete(res);
            return res != null;
        }

        public static Models.PortalExport ExportPortal(string portalId, bool includeFileContent = true)
        {
            portalId = string.IsNullOrEmpty(portalId) ? CurrentPortalId : portalId;

            var export = GetPortalExport(portalId);
            ExportAccount(portalId, export);
            ExportLocalizations(portalId, export);
            ExportTemplates(portalId, includeFileContent, export);
            return export;
        }

        private static Models.PortalExport GetPortalExport(string portalId)
        {
            var export = new Models.PortalExport();
            portalId = string.IsNullOrEmpty(portalId) ? CurrentPortalId : portalId;
            export.Portal = GetPortalById(portalId);
            return export;
        }

        public static Models.PortalExport ExportTemplates(string portalId, bool includeFileContent, Models.PortalExport export = null)
        {
            export = export != null ? export : GetPortalExport(portalId);
            if (export.Roles == null)
                export.Roles = Account.GetRoles(portalId);    //we need mapping of roles!
            if (export.SecureActivities == null)
                export.SecureActivities = Security.GetSecureActivities();    //we need mapping of roles!

            export.Manifests = GetWidgetManifests();
            export.Files = File.Get(portalId);  //todo:  somehow export file?!?!
            export.Templates = GetPageTemplates(portalId);

            export.LayoutTemplates = GetLayoutTemplates(portalId);

            //grab all widgets that have a content provider and create dictionary of <WidgetId, ContentJson>
            //TODO:  guard against no widgets with content???!?
            var allWidgets = new List<Models.Widget>();
            allWidgets.AddRange(export.Templates.SelectMany(w => w.Widgets).Where(w => w.Manifest.GetContentProvider() != null));
            allWidgets.AddRange(export.LayoutTemplates.SelectMany(w => w.Widgets).Where(w => w.Manifest.GetContentProvider() != null));
            allWidgets = allWidgets.Distinct(w => w.Id).ToList();   //since content can be shared between widgets, we only want to store it once!
            export.WidgetContent = allWidgets.ToDictionary(w => w.Id, wc => wc.GetContentJson());

            if (includeFileContent)
            {
                export.FileContent = new Dictionary<string, string>();
                foreach (var file in export.Files)
                {
                    export.FileContent[file.Id] = Portal.GetFile(file.Id).GetFileBase64();
                }
            }
            return export;
        }

        public static Models.PortalExport ExportAccount(string portalId, Models.PortalExport export = null)
        {
            export = export != null ? export : GetPortalExport(portalId);
            export.Roles = Account.GetRoles(portalId);
            export.Users = Account.GetUsers(portalId);
            export.SecureActivities = Security.GetSecureActivities();
            return export;
        }

        public static Models.PortalExport ExportLocalizations(string portalId, Models.PortalExport export = null)
        {
            export = export != null ? export : GetPortalExport(portalId);
            export.Localizations = Localization.Get(portalId).Where(l => l.Type != LocalizationType.WidgetContent).ToList();    //don't include widget content
            return export;
        }

        public static bool Import(Models.PortalExport export, string portalId)
        {
            var idMap = new Dictionary<string, string>();

            var portal = GetPortalById(portalId);
            if (portal != null)
            {
                export.Portal.Id = portal.Id;
                export.Portal.Name = portal.Name;   //IMPORTANT:  since we pass in the portalId that we want to import into, we need to ensure that is used, therefore, the name must match as that is how the duplicate lookup is done.  (i.e. {Id: 1, Name=Default} {Id: 2, Name=Foo}.  If we import Id:2 and its name importing is Default)
                export.Portal.Default = portal.Default;
            }
            else
                throw new Exception("Portal Not found: " + portalId);

            Save(export.Portal);
            portal = GetPortal(export.Portal.Name);
            SetIdMap<Models.Portal>(portal.Id, export.Portal.Id, idMap);

            if (export.Roles != null)
            {
                Logging.Logger.DebugFormat("Importing {0} roles...", export.Roles.Count);
                foreach (var role in export.Roles)
                    SetIdMap<Models.Role>(role.Id, Account.ImportRole(portal.Id, role), idMap);
            }
            if (export.Users != null)
            {
                Logging.Logger.DebugFormat("Importing {0} users...", export.Users.Count);
                foreach (var exportUser in export.Users)
                    SetIdMap<Models.User>(exportUser.Id, Account.ImportUser(portal.Id, exportUser, idMap), idMap);
            }

            if (export.SecureActivities != null)
            {
                Logging.Logger.DebugFormat("Importing {0} secure activities...", export.SecureActivities.Count);
                foreach (var exportActivity in export.SecureActivities)
                    SetIdMap<Models.SecureActivity>(exportActivity.Id, Security.Import(portal.Id, exportActivity, idMap), idMap);
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
                    SetIdMap<Models.WidgetManifest>(manifest.Id, Import(manifest), idMap);
            }
            if (export.Localizations != null)
            {
                Logging.Logger.DebugFormat("Importing {0} localizations...", export.Localizations.Count);
                foreach (var exportLocalization in export.Localizations)
                    SetIdMap<Models.Localization>(exportLocalization.Id, Localization.Import(portal.Id, exportLocalization), idMap);

            }
            if (export.Templates != null)
            {
                Logging.Logger.DebugFormat("Importing {0} page templates...", export.Templates.Count);
                foreach (var exportTemplate in export.Templates)
                    SetIdMap<Models.PageTemplate>(exportTemplate.Id, Import(portal.Id, exportTemplate, export.WidgetContent, idMap), idMap);

            }
            if (export.LayoutTemplates != null)
            {
                Logging.Logger.DebugFormat("Importing {0} layout templates...", export.LayoutTemplates.Count);
                foreach (var exportTemplate in export.LayoutTemplates)
                    SetIdMap<Models.LayoutTemplate>(exportTemplate.Id, Import(portal.Id, exportTemplate, export.WidgetContent, idMap), idMap);
            }
            return true;
        }

        private static string SetIdMap<T>(string id, string value, Dictionary<string, string> map)
        {
            var key = string.Format("{0}~{1}", typeof(T).ToString(), id);
            return map[key] = value;
        }
        public static string GetIdMap<T>(string id, Dictionary<string, string> map)
        {
            var key = string.Format("{0}~{1}", typeof(T).ToString(), id);
            return map.GetSetting<string>(key, null);
        }

        public static ConcurrentDictionary<string, List<Models.AttributeDefinition>> AttributeDefinitions = new ConcurrentDictionary<string, List<AttributeDefinition>>();
        public static bool RegisterPortalAttribute(string portalId, string groupName, Models.AttributeDefinition attribute)
        {
            var portal = GetPortalById(portalId);
            if (!AttributeDefinitions.ContainsKey(groupName))
                AttributeDefinitions[groupName] = new List<AttributeDefinition>();
            if (!AttributeDefinitions[groupName].Exists(a => a.Name.Equals(attribute.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                AttributeDefinitions[groupName].Add(attribute);
                //Save(portal);
                return true;
            }
            return false;
        }

        public static object clientIdLock = new object();
        public static string NextClientId()
        {
            lock (clientIdLock)
            {
                var id = GetRequestContextData("NextClientId", 1);
                id++;
                SetRequestContextData("NextClientId", id);
                return "w" + id;
            }
        }

        public static string ResolveUrl(string url)
        {
            return VirtualPathUtility.ToAbsolute(url);
        }
        public static string ResolveCurrentUrl(string url)
        {
            if (CurrentUrl.EndsWith("/"))
                return ResolveUrl("~/" + CurrentUrl + url);
            return ResolveUrl("~/" + CurrentUrl + "/" + url);
        }
        public static string RequestRootUrl
        {
            get
            {
                if (IsInRequest)
                    return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority).PathCombine(ResolveUrl("~/"), "/");
                return null;
            }
        }

        public static string ResolvePath(string path)
        {
            if (path.StartsWith("~/"))
                return HostingEnvironment.MapPath(path);
            return path;
        }

        public static string GetFileDir(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? CurrentPortalId : portalId;
            var dir = ResolvePath(GetFilePath());
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        public static string GetFile(string fileName)
        {
            return Path.Combine(GetFileDir(), fileName);
        }

        public static string GetTempFilePath()
        {
            return GetFilePath().PathCombine("temp", "/");
        }
        public static string GetTempFileDir()
        {
            var dir = ResolvePath(GetTempFilePath());
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        public static string GetFilePath()
        {
            return ConfigurationManager.AppSettings.GetSetting("FileDir", "~/App_Data/FileRepo");
        }

        public static string GetTempFile(string fileName = null)
        {
            fileName = string.IsNullOrEmpty(fileName) ? Guid.NewGuid() + ".tmp" : fileName;
            return Path.Combine(GetTempFileDir(), fileName);
        }

        public static List<Models.PageTemplate> GetPageTemplatesByContentId(string contentId)
        {
            return Portal.GetPageTemplates().Where(t => t.Widgets.Exists(w => w.ContentIds.Contains(contentId))).ToList();
        }
    }
}