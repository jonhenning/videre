using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainObjects = CodeEndeavors.ResourceManager.DomainObjects;
using Videre.Core.Models;
using CodeEndeavors.ResourceManager.Extensions;
using System.Web;

namespace Videre.Core.Services
{
    public class Localization
    {
        public static Models.Localization GetById(string id)
        {
            var res = Repository.GetResourceById<Models.Localization>(id);
            if (res != null)
                return res.Data;
            return null;
        }

        public static List<Models.Localization> Get(List<string> ids)
        {
            var locs = new List<Models.Localization>();
            foreach (var id in ids)
            {
                var res = Repository.GetResourceById<Models.Localization>(id);
                if (res != null)
                    locs.Add(res.Data);
            }
            return locs;
        }

        public static List<Models.Localization> Get(string portalId)
        {
            return Repository.GetResources<Models.Localization>().Select(l => l.Data).Where(l => l.PortalId == portalId).ToList();
        }

        public static List<Models.Localization> Get(LocalizationType type, string portalId, string ns)
        {
            return Repository.GetResources<Models.Localization>(type.ToString(), l => l.Data.PortalId == portalId && l.Data.Namespace.IndexOf(ns) == 0).Select(l => l.Data).ToList();
        }

        public static Models.Localization Get(string portalId, LocalizationType type, string ns, string key, string locale)
        {
            return Repository.GetResources<Models.Localization>(type.ToString(), l => 
                l.Data.PortalId == portalId && 
                l.Data.Namespace == ns && 
                l.Data.Key == key && 
                ((string.IsNullOrEmpty(l.Data.Locale) && string.IsNullOrEmpty(locale) ) || (l.Data.Locale == locale ))
                ).Select(l => l.Data).SingleOrDefault();
        }
        
        public static List<Models.Localization> Get(LocalizationType type, string portalId)
        {
            return Repository.GetResources<Models.Localization>(type.ToString(), l => l.Data.PortalId == portalId).Select(l => l.Data).ToList();
        }

        //since currently part of content provider, place in service 
        public static string Import(string portalId, Models.Localization localization, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;

            if (localization.Type == LocalizationType.Portal)
                localization.Namespace = portalId;   //type portal uses portalid as namespace

            var existing = Localization.Get(portalId, localization.Type, localization.Namespace, localization.Key, localization.Locale);

            localization.PortalId = portalId;
            localization.Id = existing != null ? existing.Id : null;
            return Save(localization, userId);
        }

        public static string Save(Models.Localization localization, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            localization.PortalId = string.IsNullOrEmpty(localization.PortalId) ? Services.Portal.CurrentPortalId : localization.PortalId;
            if (!IsDuplicate(localization))
            {
                var res = Repository.StoreResource(localization.Type.ToString(), localization.Key, localization, userId);
                return res.Id;
            }
            else
                throw new Exception(GetExceptionText("DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "Localization"));
        }

        //make sure you treat like a single instance... no expiring, etc.
        private static bool IsDuplicate(Models.Localization localization)
        {
            var items = Repository.GetResources<Models.Localization>(localization.Type.ToString(), localization.Key,
                l => l.Data.Locale == localization.Locale && l.Data.PortalId == localization.PortalId && l.Data.Namespace == localization.Namespace);
            return items.Exists(l => l.Id != localization.Id);
                //    ).Exists(l => l.Id != localization.Id);
                    //&& l.Data.Type != LocalizationType.WidgetContent).Exists(l => l.Id != localization.Id);
        }

        public static bool Delete(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var localization = Repository.GetResourceById<Models.Localization>(id);
            if (localization != null)
                Repository.Delete(localization);
            return localization != null;
        }

        public static string GetExceptionText(string key, string defaultText)
        {
            return GetExceptionText(key, defaultText, new string[0]);
        }

        public static string GetExceptionText(string key, string defaultText, params object[] args)
        {
            return string.Format(Localization.GetLocalization(Videre.Core.Models.LocalizationType.Exception, key, defaultText, "Core"), args);
        }

        //public static string GetLocalization(LocalizationType type, string key, string defaultText, string ns)
        //{
        //    return GetLocalization(type, key, defaultText, ns, CurrentUserLocale, Services.Portal.CurrentPortalId.ToString());
        //}

        //public static string GetLocalization(LocalizationType type, string key, string defaultText, string ns, string locale, string portalId)
        //{
        //    return GetLocalization(type, key, defaultText, ns, locale, portalId, true);
        //}

        public static string GetPortalText(string key, string defaultValue)
        {
            if (!Portal.StartupFailed)
                return Services.Localization.GetLocalization(LocalizationType.Portal, key, defaultValue, Services.Portal.CurrentPortalId, portalId: Services.Portal.CurrentPortalId);
            return defaultValue;
        }

        public static string GetLocalization(LocalizationType type, string key, string defaultText, string ns, string locale = null, string portalId = null, bool create = true)
        {
            if (!string.IsNullOrEmpty(key))
            {
                locale = string.IsNullOrEmpty(locale) ? CurrentUserLocale : locale;
                portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
                var text = defaultText;

                var loc = Repository.GetResourceData<Models.Localization>(type.ToString(), key, GetLocalizationQueries(portalId, ns, locale), null);
                if (loc == null && create)
                    Services.Localization.Save(new Models.Localization() { Type = type, Key = key, Namespace = ns, Locale = null, PortalId = portalId, Text = defaultText }, Services.Account.AuditId);
                else if (loc != null)
                    text = loc.Text;

                return text;
            }
            return null;
        }

        private static List<DomainObjects.Query<Models.Localization>> GetLocalizationQueries(string locale)
        {
            return new List<DomainObjects.Query<Models.Localization>>()
                    {new DomainObjects.Query<Models.Localization>(l => l.Locale == locale, 3), 
                    new DomainObjects.Query<Models.Localization>(l => (locale.Length > 2 && l.Locale == locale.Substring(0, 2)), 2), 
                    new DomainObjects.Query<Models.Localization>(l => string.IsNullOrEmpty(l.Locale), 1)};
        }

        private static List<DomainObjects.Query<DomainObjects.Resource<Models.Localization>>> GetLocalizationQueries(string portalId, string ns, string locale)
        {
            return new List<DomainObjects.Query<DomainObjects.Resource<Models.Localization>>>()
                    {new DomainObjects.Query<DomainObjects.Resource<Models.Localization>>(l => l.Data.PortalId == portalId && l.Data.Namespace == ns && l.Data.Locale == locale, 3), 
                    new DomainObjects.Query<DomainObjects.Resource<Models.Localization>>(l => l.Data.PortalId == portalId && l.Data.Namespace ==ns && (locale.Length > 2 && l.Data.Locale == locale.Substring(0, 2)), 2), 
                    new DomainObjects.Query<DomainObjects.Resource<Models.Localization>>(l => l.Data.PortalId == portalId && l.Data.Namespace == ns && string.IsNullOrEmpty(l.Data.Locale), 1)};
        }

        public static string GetContent(List<Models.Localization> content, string key, string defaultValue)
        {
            var queries = GetLocalizationQueries(CurrentUserLocale);
            var items = content.Where(c => c.Key == key);
            var matches = queries.GetMatches(items, true);
            if (matches.Count > 0)
                return matches[0].DisplayText;
            return defaultValue;
            
            //var content = Repository.Get(Content.Select(c => c.ToResource()).ToList(), GetLocalizationQueries(CurrentUserLocale), true);
            //if (content.Count > 0)
            //    return content[0].Data;
            //return DefaultValue;
        }

        public static List<Models.Localization> GetLocalizations(LocalizationType type, Func<Models.Localization, bool> query, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId; 
            //var locs = Get(type, portalId).Where(query);
            var queries = GetLocalizationQueries(portalId, portalId, CurrentUserLocale); //GetLocalizationQueries(CurrentUserLocale);
            var resourceLocs = Repository.GetResources<Models.Localization>(type.ToString(), l => l.Data.PortalId == portalId && l.Data.Key.EndsWith(".Client"));
            return Repository.GetResources<Models.Localization>(resourceLocs, queries, true).Select(r => r.Data).ToList();
            //return queries.GetMatches(locs, true);
        }

        public static string CurrentUserLocale
        {
            get
            {
                var locale = "";
                try
                {
                    if (Portal.IsInRequest && HttpContext.Current.Request.UserLanguages != null && HttpContext.Current.Request.UserLanguages.Count() > 0)
                        locale = HttpContext.Current.Request.UserLanguages.FirstOrDefault();
                }
                catch (Exception)    //todo: lame to do exception trapping here!
                {
                    //ignore
                }
                if (Services.Account.CurrentUser != null && !string.IsNullOrEmpty(Services.Account.CurrentUser.Locale))
                    locale = Services.Account.CurrentUser.Locale;
                return locale;
            }
        }

    }
}
