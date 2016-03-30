using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using CodeEndeavors.Extensions;
using CodeEndeavors.ResourceManager.DomainObjects;
using CodeEndeavors.ResourceManager.Extensions;
using Videre.Core.Extensions;
using Videre.Core.Models;

namespace Videre.Core.Services
{
    public static class Portal
    {
        private static ConcurrentDictionary<string, List<AttributeDefinition>> _attributeDefinitions = null;

        public static ConcurrentDictionary<string, List<AttributeDefinition>> AttributeDefinitions 
        {
            get
            {
                if (_attributeDefinitions == null)
                {
                    var definitions = Repository.GetResources<AttributeDefinition>("AttributeDefinition").Select(a => a.Data).GroupBy(a => a.GroupName).ToDictionary(a => a.Key, a => a.ToList());
                    if (definitions != null)
                        _attributeDefinitions = definitions.ToJson().ToObject<ConcurrentDictionary<string, List<AttributeDefinition>>>();
                    if (_attributeDefinitions == null)
                        _attributeDefinitions = new ConcurrentDictionary<string, List<AttributeDefinition>>();
                }
                return _attributeDefinitions;
            }
        }

        public static object clientIdLock = new object();

        public static bool IsInRequest
        {
            get
            {
                try
                {
                    return HttpContext.Current != null && HttpContext.Current.Handler != null && HttpContext.Current.Request != null;
                }
                catch (Exception) //todo: why would this occur?
                {
                    return false;
                }
            }
        }

        public static string TempDir
        {
            get
            {
                var tempDir = ResolvePath(GetAppSetting("TempDir", "~/_temp/"));
                if (!Directory.Exists(tempDir)) //todo: do this each time???
                    Directory.CreateDirectory(tempDir);
                return tempDir;
            }
        }

        //wrapping these calls to potentially enforce prefixing of keys... 

        public static PageTemplate CurrentTemplate //todo: force ViewBag use of use HttpContext?
        {
            get { return GetRequestContextData<PageTemplate>("CurrentTemplate", null); }
            set { SetRequestContextData("CurrentTemplate", value); }
        }

        public static string CurrentUrl
        {
            get { return GetRequestContextData("CurrentUrl", ""); }
            set { SetRequestContextData("CurrentUrl", value); }
        }

        public static Dictionary<string, string> CurrentUrlMatchedGroups
        {
            get { return GetRequestContextData("CurrentUrlMatchedGroups", new Dictionary<string, string>()); }
            set { SetRequestContextData("CurrentUrlMatchedGroups", value); }
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
                //return CacheState.PullRequestCache("CurrentPortal", delegate
                return Caching.GetRequestCacheEntry("CurrentPortal", () =>
                {
                    var portals = GetPortals();
                    string bestMatch = ""; 
                    if (RequestRootUrl != null) //during an update (app start) url will be null, just pull default in this case
                        bestMatch = RouteParser.GetBestMatchedUrl(RequestRootUrl, portals.SelectMany(t => t.Aliases));
                    Models.Portal portal = null;
                    if (!string.IsNullOrEmpty(bestMatch))
                        portal = portals.FirstOrDefault(t => t.Aliases.Contains(bestMatch));
                    return portal ?? GetDefaultPortal();
                });
            }
        }

        public static string RequestRootUrl
        {
            get { return IsInRequest ? HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority).PathCombine(ResolveUrl("~/"), "/") : null; }
        }

        public static T GetRequestContextData<T>(string key, T defaultValue)
        {
            return IsInRequest ? HttpContext.Current.Items.GetSetting(key, defaultValue) : defaultValue;
        }

        public static void SetRequestContextData<T>(string key, T value)
        {
            if (IsInRequest) //todo: allow this to be on thread?
                HttpContext.Current.Items[key] = value;
        }

        public static T GetCurrentUrlSegment<T>(string name, T defaultValue, bool throwError = false)
        {
            if (!CurrentUrlMatchedGroups.ContainsKey(name) && throwError)
                throw new Exception(
                    string.Format(
                        Localization.GetLocalization(LocalizationType.Exception, "UrlGroupNotFound.Error",
                            "The url for this template was expected to have a segment with {{{0}:datatype}} in it",
                            "Core"), name));

            return CurrentUrlMatchedGroups.GetSetting(name, defaultValue);
        }

        public static PageTemplate GetPageTemplateFromUrl(string url, string portalId = null)
        {
            var temp = Widget.GetWidgetManifests(); //ensure manifests are loaded

            portalId = string.IsNullOrEmpty(portalId) ? CurrentPortalId : portalId;
            //todo:  use widgets to determine authentication...
            //if (!Services.Account.IsAuthenticated)
            //    Url = "Account/LogOn";

            var template = GetMatchedTemplate(url, portalId) ?? GetMatchedTemplate("", portalId);

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

        public static List<PageTemplate> GetPageTemplates(string portalId = null)
        {
            portalId = portalId.CoalesceString(CurrentPortalId);
            return Repository.GetResources<PageTemplate>("Template", t => t.Data.PortalId == portalId)
                .Select(t => t.Data)
                .OrderBy(t => t.Urls.FirstOrDefault())
                .ToList();
        }

        public static PageTemplate GetPageTemplateById(string id)
        {
            var res = Repository.GetResourceById<PageTemplate>(id);
            return res != null ? res.Data : null;
        }

        public static PageTemplate GetPageTemplate(string url, string portalId = null)
        {
            return GetPageTemplates(url, portalId).FirstOrDefault();
        }

        public static List<PageTemplate> GetPageTemplates(string url, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? CurrentPortalId : portalId;
            var templates = GetPageTemplates(portalId);
            return templates.Where(t2 => ((string.IsNullOrEmpty(url) && t2.Urls.Count == 0) || t2.Urls.Contains(url)) && t2.PortalId == portalId).ToList();
        }

        public static PageTemplate GetMatchedTemplate(string url, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? CurrentPortalId : portalId;

            var templates = Repository.GetResources<PageTemplate>("Template", t => t.Data.PortalId == portalId).Select(t => t.Data);
            if (!string.IsNullOrEmpty(url))
            {
                var bestMatch = RouteParser.GetBestMatchedUrl(url, templates.SelectMany(t => t.Urls));
                if (!string.IsNullOrEmpty(bestMatch))
                {
                    if (IsInRequest)
                        //todo: only need this when in request, otherwise ignore... perhaps should be moved out to controller...
                    {
                        CurrentUrlMatchedGroups = RouteParser.Parse(bestMatch, url);
                        CurrentUrl = CurrentUrlMatchedGroups["0"];
                    }
                    return getBestOfMultipleMatched(templates.Where(t => t.Urls.Contains(bestMatch)));
                }
            }
            CurrentUrl = string.Empty;
            return getBestOfMultipleMatched(templates.Where(t => t.Urls.Count == 0)); //grab default template
        }

        private static PageTemplate getBestOfMultipleMatched(IEnumerable<PageTemplate> templates)
        {
            //if multiple matches, then check to see if at least one is authorized, and if so filter on it
            if (templates.Count() > 1 && templates.Where(t => t.IsAuthorized).Count() > 0)
                templates = templates.Where(t => t.IsAuthorized);
            return templates.FirstOrDefault();
        }

        [Obsolete("Use RouteParser.GetBestMatchedUrl instead")]
        public static string GetBestMatchedUrl(string url, IEnumerable<string> urls)
        {
            return RouteParser.GetBestMatchedUrl(url, urls);
        }
        //public static string GetBestMatchedUrl(string url, IEnumerable<string> urls)
        //{
        //    var queries = new List<Query<string>>
        //    {
        //        new Query<string>(u => RouteParser.Parse(u, url).Keys.Count > 0, 1)
        //    };

        //    var matchedUrls = queries.GetMatches(urls, false);
        //    if (matchedUrls.Count > 0)
        //    {
        //        //our most specific match is determined by number of matching groups from regex... - if tie then use length of matchedUrl
        //        return (from u in matchedUrls
        //            orderby RouteParser.Parse(u, url).Keys.Count descending, u.Length descending
        //            select u).FirstOrDefault();
        //    }
        //    return null;
        //}

        //grab content ids that are used by more than one widget
        public static List<string> GetSharedContentIds()
        {
            var ids = GetPageTemplates().SelectMany(t => t.Widgets).SelectMany(w => w.ContentIds).ToList();
            ids.AddRange(GetLayoutTemplates().SelectMany(t => t.Widgets).SelectMany(w => w.ContentIds));
            return ids.GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        }

        public static string Save(PageTemplate pageTemplate, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            pageTemplate.PortalId = string.IsNullOrEmpty(pageTemplate.PortalId)
                ? CurrentPortalId
                : pageTemplate.PortalId;
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
                Repository.StoreResource("Template", null, pageTemplate, userId);

                foreach (var widget in pageTemplate.Widgets)
                {
                    //widget.TemplateId = Template.Id;    //is this a hack?
                    if (widget.ContentJson != null)
                        widget.SaveContentJson(widget.ContentJson);
                }

                //after contentIds assigned, need to save them!
                var res = Repository.StoreResource("Template", null, pageTemplate, userId);
                return res.Id;
            }
            throw new Exception(string.Format(
                Localization.GetLocalization(LocalizationType.Exception, "DuplicateResource.Error",
                    "{0} already exists.   Duplicates Not Allowed.", "Core"), "Template"));
        }

        public static int RegisterPageTemplate(string title, string url, string layoutName, string widgetPaneName, string widgetManifestFullName, bool? authenticated = null)
        {
            var layout = GetLayoutTemplate(CurrentPortalId, layoutName);
            if (layout == null)
                throw new Exception("Layout not found: " + layoutName);
            var manifest = Widget.GetWidgetManifest(widgetManifestFullName);
            if (manifest == null)
                throw new Exception("Widget Manifest not found: " + widgetManifestFullName);

            var newTemplate = new PageTemplate()
            {
                Title = title,
                Urls = new List<string>() { url },
                LayoutId = layout.Id,
                Authenticated = authenticated,
                Widgets = new List<Models.Widget>()
                {
                    new Models.Widget() { ManifestId = manifest.Id, PaneName = widgetPaneName }
                }};

            return RegisterPageTemplate(newTemplate);
        }

        public static int RegisterPageTemplate(string title, string url, string layoutName, Dictionary<string, string> paneWidgetFulleNameDict, bool? authenticated)
        {
            var layout = GetLayoutTemplate(CurrentPortalId, layoutName);
            if (layout == null)
                throw new Exception("Layout not found: " + layoutName);

            var newTemplate = new PageTemplate()
            {
                Title = title,
                Urls = new List<string>() { url },
                LayoutId = layout.Id,
                Authenticated = authenticated,
                Widgets = paneWidgetFulleNameDict.Keys.Select(widgetPaneName =>
                {
                    var manifest = Widget.GetWidgetManifest(paneWidgetFulleNameDict[widgetPaneName]);
                    if (manifest == null)
                        throw new Exception("Widget Manifest not found: " + paneWidgetFulleNameDict[widgetPaneName]);
                    return new Models.Widget() { ManifestId = manifest.Id, PaneName = widgetPaneName };
                }).ToList()
            };

            return RegisterPageTemplate(newTemplate);

        }

        public static int RegisterPageTemplate(Models.PageTemplate template)
        {
            var updated = false;
            var saveTemplate = Portal.GetPageTemplate(template.Urls.FirstOrDefault());
            if (saveTemplate == null)
            {
                saveTemplate = template;
                updated = true;
            }
            else
                updated = saveTemplate.Merge(template);

            if (updated)
            {
                Portal.Save(saveTemplate);
                return 1;
            }
            return 0;
        }


        public static bool DeletePageTemplate(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var res = Repository.GetResourceById<PageTemplate>(id);
            if (res != null)
            {
                //foreach (var widget in res.Data.Widgets)
                //    widget.DeleteContent();
                Repository.Delete(res);
            }
            return res != null;
        }

        public static bool Exists(PageTemplate pageTemplate)
        {
            var templates = GetPageTemplates();
            return pageTemplate.IsDefault
                ? templates.Exists(t => t.IsDefault)
                : pageTemplate.Urls.Select(url => templates.FirstOrDefault(t2 => t2.Urls.Contains(url) && t2.PortalId == pageTemplate.PortalId)).Any(t => t != null);
        }

        public static bool IsDuplicate(PageTemplate pageTemplate)
        {
            var templates = GetPageTemplates(pageTemplate.PortalId);

            if (pageTemplate.IsDefault)
                return templates.Exists(t => t.IsDefault && (t.Authenticated == pageTemplate.Authenticated || t.Authenticated == null || pageTemplate.Authenticated == null) && t.Id != pageTemplate.Id);
            else
                return pageTemplate.Urls.Select(url => templates.FirstOrDefault(t2 => t2.Urls.Contains(url)
                    && t2.PortalId == pageTemplate.PortalId
                    && (t2.Authenticated == pageTemplate.Authenticated || t2.Authenticated == null || pageTemplate.Authenticated == null)   //for now allow duplicate urls as long as authentication different
                    && t2.Id != pageTemplate.Id
                    )).Any(t => t != null);
        }

        public static List<LayoutTemplate> GetLayoutTemplates(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? CurrentPortalId : portalId;
            return Repository.GetResources<LayoutTemplate>("LayoutTemplate", t => t.Data.PortalId == portalId, false).Select(t => t.Data).OrderBy(t => t.LayoutName).ToList();
        }

        public static LayoutTemplate GetLayoutTemplateById(string id)
        {
            var res = Repository.GetResourceById<LayoutTemplate>(id);
            return res != null ? res.Data : null;
        }

        public static LayoutTemplate GetLayoutTemplate(string layoutName)
        {
            var template = Repository.GetResources<LayoutTemplate>("LayoutTemplate", t => t.Data.PortalId == CurrentPortalId && t.Data.LayoutName == layoutName, true).SingleOrDefault();
            return template != null ? template.Data : null;
        }

        public static LayoutTemplate GetLayoutTemplate(string portalId, string layoutName)
        {
            var template = Repository.GetResources<LayoutTemplate>("LayoutTemplate", t => t.Data.PortalId == portalId && t.Data.LayoutName == layoutName, true).SingleOrDefault();
            return template != null ? template.Data : null;
        }


        public static string Save(LayoutTemplate template, string userId = null)
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

                Repository.StoreResource("LayoutTemplate", null, template, userId);
                foreach (var widget in template.Widgets)
                {
                    //widget.TemplateId = Template.Id;    //is this a hack?
                    if (widget.ContentJson != null)
                        //needed for initial update where we want to assign ContentIds and not json...   may cause issue with blanking out contentids by trying to blank out contentjson... TODO:
                        widget.SaveContentJson(widget.ContentJson);
                }
                //after contentIds assigned, need to save them!
                var res = Repository.StoreResource("LayoutTemplate", null, template, userId);
                return res.Id;
            }
            throw new Exception( string.Format(
                Localization.GetLocalization(LocalizationType.Exception, "DuplicateResource.Error",
                "{0} already exists.   Duplicates Not Allowed.", "Core"), "LayoutTemplate"));
        }

        public static int RegisterLayoutTemplate(string layoutName, string layoutViewName, string themeName, string widgetPaneName, string widgetManifestFullName)
        {
            var manifest = Widget.GetWidgetManifest(widgetManifestFullName);
            if (manifest == null)
                throw new Exception("Widget Manifest not found: " + widgetManifestFullName);

            var newTemplate = new LayoutTemplate()
            {
                LayoutName = layoutName,
                LayoutViewName = layoutViewName,
                ThemeName = themeName,
                Widgets = new List<Models.Widget>()
                {
                    new Models.Widget() { ManifestId = manifest.Id, PaneName = widgetPaneName }
                }
            };

            return RegisterLayoutTemplate(newTemplate);
        }

        public static int RegisterLayoutTemplate(string layoutName, string layoutViewName, string themeName, Dictionary<string, string> paneWidgetFulleNameDict)
        {
            var newTemplate = new LayoutTemplate()
            {
                LayoutName = layoutName,
                LayoutViewName = layoutViewName,
                ThemeName = themeName,
                Widgets = paneWidgetFulleNameDict.Keys.Select(widgetPaneName => 
                    {
                        var manifest = Widget.GetWidgetManifest(paneWidgetFulleNameDict[widgetPaneName]);
                        if (manifest == null)
                            throw new Exception("Widget Manifest not found: " + paneWidgetFulleNameDict[widgetPaneName]);
                        return new Models.Widget() { ManifestId = manifest.Id, PaneName = widgetPaneName };
                    }).ToList()
            };
            return RegisterLayoutTemplate(newTemplate);
        }

        public static int RegisterLayoutTemplate(Models.LayoutTemplate template)
        {
            var updated = false;
            var saveTemplate = Portal.GetLayoutTemplate(template.LayoutName);
            if (saveTemplate == null)
            {
                saveTemplate = template;
                updated = true;
            }
            else
                updated = saveTemplate.Merge(template);

            if (updated)
            {
                Portal.Save(saveTemplate);
                return 1;
            }
            return 0;
        }

        public static bool DeleteLayoutTemplate(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var res = Repository.GetResourceById<LayoutTemplate>(id);
            if (res != null)
            {
                //foreach (var widget in res.Data.Widgets)
                //    widget.DeleteContent();
                Repository.Delete(res);
            }
            return res != null;
        }

        public static bool Exists(LayoutTemplate template)
        {
            return GetLayoutTemplate(template.PortalId, template.LayoutName) != null;
        }

        public static bool IsDuplicate(LayoutTemplate template)
        {
            var t = GetLayoutTemplate(template.PortalId, template.LayoutName);
            return t != null && t.Id != template.Id;
        }

        public static List<Models.Portal> GetPortals()
        {
            return Repository.GetResources<Models.Portal>("Portal").Select(m => m.Data).ToList();
        }

        public static Models.Portal GetPortalById(string id)
        {
            return GetPortals().FirstOrDefault(m => m.Id == id);
        }

        public static Models.Portal GetPortal(string name)
        {
            return GetPortals().FirstOrDefault(m => m.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public static Models.Portal GetDefaultPortal()
        {
            return GetPortals().FirstOrDefault(m => m.Default);
        }

        public static bool Save(Models.Portal portal, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            if (!IsDuplicate(portal))
            {
                var portals = GetPortals();
                if (portal.Default)
                {
                    foreach (var p in portals.Where(p => p.Default && p.Id != portal.Id))
                    {
                        p.Default = false;
                        Repository.StoreResource("Portal", null, p, userId);
                    }
                }
                else if (!portals.Exists(p => p.Default && p.Id != portal.Id))
                    throw new Exception(
                        string.Format(
                            Localization.GetLocalization(LocalizationType.Exception, "DefaultPortalRequired.Error",
                                "At least one portal must be marked as the default.", "Core"), "Portal"));

                Repository.StoreResource("Portal", null, portal, userId);
            }
            else
                throw new Exception(
                    string.Format(
                        Localization.GetLocalization(LocalizationType.Exception, "DuplicateResource.Error",
                            "{0} already exists.   Duplicates Not Allowed.", "Core"), "Portal"));
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
            var res = Repository.GetResourceById<Models.Portal>(id);
            if (res != null)
                Repository.Delete(res);
            return res != null;
        }

        [Obsolete("Use RegisterPortalAttribute(attribute) instead.  GroupName now on Attribute Model")]
        public static bool RegisterPortalAttribute(string groupName, AttributeDefinition attribute)
        {
            attribute.GroupName = groupName;
            return RegisterPortalAttribute(attribute);
        }

        public static bool RegisterPortalAttribute(AttributeDefinition attribute)
        {
            var changed = false;
            //var portal = GetPortalById(portalId);

            if (!AttributeDefinitions.ContainsKey(attribute.GroupName))
                AttributeDefinitions[attribute.GroupName] = new List<AttributeDefinition>();

            var def = AttributeDefinitions[attribute.GroupName].Where(a => a.Name.Equals(attribute.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (def == null)
            {
                AttributeDefinitions[attribute.GroupName].Add(attribute);
                changed = true;
            }
            else 
            {
                attribute.Id = def.Id;
                if (def.ToJson() != attribute.ToJson())
                {
                    AttributeDefinitions[attribute.GroupName][AttributeDefinitions[attribute.GroupName].IndexOf(def)] = attribute;
                    changed = true;
                }
            }

            if (changed)
                Repository.StoreResource("AttributeDefinition", null, attribute, Account.AuditId);
            return changed;
        }

        public static bool UnregisterPortalAttribute(string groupName, string name)
        {
            if (AttributeDefinitions.ContainsKey(groupName))
            {
                var def = AttributeDefinitions[groupName].Where(a => a.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (def != null)
                {
                    AttributeDefinitions[groupName].Remove(def);
                    var res = Repository.GetResourceById<Models.SecureActivity>(def.Id);
                    if (res != null)
                    {
                        Repository.Delete(res);
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool AttributeDefinitionChanged(AttributeDefinition attribute)
        {
            if (!AttributeDefinitions.ContainsKey(attribute.GroupName))
                AttributeDefinitions[attribute.GroupName] = new List<AttributeDefinition>();
            var def = AttributeDefinitions[attribute.GroupName].Where(a => a.Name.Equals(attribute.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (def != null)
                return def.ToJson() != attribute.ToJson();  //todo: not sure this is reliable
            else
                return true;
        }


        public static T GetPortalAttribute<T>(string groupName, string name, T defaultValue)
        {
            if (CurrentPortal != null)
                return CurrentPortal.GetAttribute(groupName, name, defaultValue);
            return defaultValue;
        }

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
            if (!string.IsNullOrEmpty(url))
                return VirtualPathUtility.ToAbsolute(url);
            return "";
        }

        public static string ResolveCurrentUrl(string url)
        {
            return CurrentUrl.EndsWith("/") ? ResolveUrl("~/" + CurrentUrl + url) : ResolveUrl("~/" + CurrentUrl + "/" + url);
        }

        public static string ResolvePath(string path)
        {
            return path.StartsWith("~/") ? HostingEnvironment.MapPath(path) : path;
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

        public static T GetAppSetting<T>(string key, T defaultValue)
        {
            return ConfigurationManager.AppSettings.GetSetting<T>(key, defaultValue);
        }

        public static T GetPortalSetting<T>(string groupName, string key, T defaultValue)
        {
            if (Portal.CurrentPortal != null)
                return Portal.CurrentPortal.GetAttribute(groupName, key, defaultValue);
            return defaultValue;
        }

        public static string GetFilePath()
        {
            return GetAppSetting("FileDir", "~/App_Data/FileRepo");
        }

        public static string GetContentPath()
        {
            return "~/content";
        }

        public static string GetTempFile(string fileName = null)
        {
            fileName = string.IsNullOrEmpty(fileName) ? Guid.NewGuid() + ".tmp" : fileName;
            return Path.Combine(GetTempFileDir(), fileName);
        }

        public static List<PageTemplate> GetPageTemplatesByContentId(string contentId)
        {
            return GetPageTemplates().Where(t => t.Widgets.Exists(w => w.ContentIds.Contains(contentId))).ToList();
        }
    }
}