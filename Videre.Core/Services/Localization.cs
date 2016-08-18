using CodeEndeavors.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainObjects = CodeEndeavors.ResourceManager.DomainObjects;
using Videre.Core.Models;
using CodeEndeavors.ResourceManager.Extensions;
using System.Web;
using CodeEndeavors.ResourceManager.DomainObjects;
using System.IO;

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
                ((string.IsNullOrEmpty(l.Data.Locale) && string.IsNullOrEmpty(locale)) || (l.Data.Locale == locale))
                ).Select(l => l.Data).FirstOrDefault();
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

        public static int Import(string portalId, List<Models.Localization> localizations, string userId = null)
        {
            int updated = 0;
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;

            var existingLocalizations = Localization.Get(portalId);

            foreach (var localization in localizations)
            {
                if (localization.Type == LocalizationType.Portal)
                    localization.Namespace = portalId;   //type portal uses portalid as namespace

                var existing = existingLocalizations.Where(l =>
                    l.PortalId == portalId &&
                    l.Namespace == localization.Namespace &&
                    l.Key == localization.Key &&
                    ((string.IsNullOrEmpty(l.Locale) && string.IsNullOrEmpty(localization.Locale)) || l.Locale == localization.Locale)).FirstOrDefault();

                localization.PortalId = portalId;
                localization.Id = existing != null ? existing.Id : null;
                if (existing == null || existing.Text != localization.Text)
                {
                    if (string.IsNullOrEmpty(localization.Text))
                    {
                        if (existing != null)
                        {
                            Delete(existing.Id, userId);
                            updated++;
                        }
                    }
                    else
                    {
                        Save(localization, userId);
                        updated++;
                    }
                }
            }
            return updated;
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

        public static List<Models.Localization> GetLocalizations(LocalizationType type, string portalId = null)
        {
            return GetLocalizations(type, null, portalId);
        }
        public static List<Models.Localization> GetLocalizations(LocalizationType type, Func<Resource<Models.Localization>, bool> query, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            //var locs = Get(type, portalId).Where(query);
            var queries = GetLocalizationQueries(portalId, portalId, CurrentUserLocale); //GetLocalizationQueries(CurrentUserLocale);

            //combine lambdas
            Func<Resource<Models.Localization>, bool> portalQuery = (l => l.Data.PortalId == portalId);
            Func<Resource<Models.Localization>, dynamic> combinedQuery = l => portalQuery(l);
            if (query != null)
                combinedQuery = (l => query(l) && portalQuery(l));

            var resourceLocs = Repository.GetResources<Models.Localization>(type.ToString(), combinedQuery);
            return Repository.GetResources<Models.Localization>(resourceLocs, queries, true).Select(r => r.Data).ToList();
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

        public static byte[] GenerateLanguagePackFile(LocalizationType type, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            var localizations = Repository.GetResources<Models.Localization>(type.ToString(), l => l.Data.PortalId == portalId).Select(l => l.Data).ToList();
            var locales = localizations.Select(l => l.Locale).Distinct();
            var locHierarchy = localizations.Where(l => l.Namespace != null).GroupBy(l => l.Namespace).ToDictionary(l => l.Key, l => l.GroupBy(l2 => l2.Key).ToDictionary(l3 => l3.Key));

            using (var tw = new StringWriter())
            {
                //resorting to tab separated output due to the fact that Microsoft still doesn't support UTF8 directly (quite unbelievable)... - https://excel.uservoice.com/forums/304921-excel-for-windows-desktop-application/suggestions/10006149-support-saving-csv-in-utf-8-encoding
                var csv = new CsvHelper.CsvWriter(tw, new CsvHelper.Configuration.CsvConfiguration() { Encoding = Encoding.UTF8 });
                csv.WriteField(type.ToString());
                csv.NextRecord();
                csv.WriteField("Namespace");
                csv.WriteField("Code");

                foreach (var locale in locales)
                    csv.WriteField(locale);
                csv.NextRecord();
                foreach (var ns in locHierarchy.Keys)
                {
                    foreach (var key in locHierarchy[ns].Keys)
                    {
                        csv.WriteField(ns);
                        csv.WriteField(key);
                        var locs = locHierarchy[ns][key];
                        foreach (var locale in locales)
                        {
                            var loc = locs.Where(l => l.Locale == locale).FirstOrDefault();
                            if (loc != null)
                                csv.WriteField(loc.Text);
                            else
                                csv.WriteField("");
                        }
                        csv.NextRecord();
                    }
                }
                //return System.Text.Encoding.UTF8.GetBytes(tw.ToString());
                return Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(tw.ToString())).ToArray();//http://stackoverflow.com/questions/4414088/how-to-getbytes-in-c-sharp-with-utf8-encoding-with-bom/4414118#4414118
            }

        }

        public static int? ApplyLanguagePackFile(string data, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            int? processedRows = null;

            using (var tr = new StringReader(data))
            {
                var delimiter = ",";
                //hack to determine if using tab delimiter.
                var rows = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                if (rows.Length > 1 && rows[1].IndexOf('\t') > -1)
                    delimiter = "\t";

                var csv = new CsvHelper.CsvReader(tr, new CsvHelper.Configuration.CsvConfiguration() { Encoding = Encoding.UTF8, HasHeaderRecord = false, Delimiter = delimiter });
                if (csv.Read())
                {
                    var type = (LocalizationType)Enum.Parse(typeof(LocalizationType), csv.GetField<string>(0));
                    if (csv.Read())
                    {
                        var locales = new List<string>();
                        var col = 2;
                        string locale;
                        while (csv.TryGetField<string>(col, out locale))
                        {
                            locales.Add(locale);
                            col++;
                        }

                        if (locales.Count == 0)
                            throw new Exception(Localization.GetExceptionText("InvalidFile.Error", "Invalid File.  {0}", "Locales Missing"));

                        var localizations = new List<Models.Localization>();

                        while (csv.Read())
                        {
                            string translation;
                            for (var i = 0; i < locales.Count; i++)
                            {
                                if (csv.TryGetField<string>(i + 2, out translation))
                                {
                                    locale = String.IsNullOrEmpty(locales[i]) ? null : locales[i];
                                    var ns = type == LocalizationType.Portal ? portalId : csv.GetField<string>(0); //namespace is sometimes PortalId - in those cases we need to override this!!!!
                                    var key = csv.GetField<string>(1);
                                    var localization = new Models.Localization()
                                    {
                                        PortalId = portalId,
                                        Type = type,
                                        Namespace = ns,
                                        Key = key,
                                        Locale = locale,
                                        Text = translation
                                    };

                                    localizations.Add(localization);
                                }
                            }
                        }
                        processedRows = Import(portalId, localizations);
                    }
                    else
                        throw new Exception(Localization.GetExceptionText("InvalidFile.Error", "Invalid File.  {0}", "Locales Missing"));
                }
            }
            return processedRows;
        }

        private static string escapeCsvText(string text)
        {
            if (text.Contains("\""))
                text = String.Format("\"{0}\"", text.Replace("\"", "\"\""));
            if (text.Contains(","))
                text = String.Format("\"{0}\"", text);

            if (text.Contains(System.Environment.NewLine))
                text = String.Format("\"{0}\"", text);

            return text;
        }

    }
}
