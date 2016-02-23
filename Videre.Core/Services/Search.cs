using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using Videre.Core.Models;
using CodeEndeavors.Extensions;
using System.Configuration;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace Videre.Core.Services
{
    public class Search
    {
        private static ConcurrentDictionary<string, ISearchProvider> _searchServices = new ConcurrentDictionary<string, ISearchProvider>();
        private static ConcurrentDictionary<string, ISearchProvider> _documentProviders = new ConcurrentDictionary<string, ISearchProvider>();

        public static string IndexDir
        {
            get
            {
                return Portal.ResolvePath(Videre.Core.Services.Portal.CurrentPortal.GetAttribute("Core", "SearchIndexDir", "~/App_Data/SearchIndex"));
            }
        }

        public static ISearchProvider GetService(string name)
        {
            var provider = GetSearchProvider(name);
            if (provider != null)
            {
                if (!_searchServices.ContainsKey(name))
                    _searchServices[name] = provider.ProviderType.GetInstance<ISearchProvider>();
            }
            return _searchServices[name];
        }

        private static ISearchProvider GetDocumentProvider(string documentType)
        {
            if (!_documentProviders.ContainsKey(documentType))
            {
                var provider = GetSearchProviders().Where(p => p.DocumentTypes.Contains(documentType)).FirstOrDefault();
                if (provider != null)
                    _documentProviders[documentType] = GetService(provider.Name);
                else
                    _documentProviders[documentType] = null;
            }
            return _documentProviders[documentType];
        }

        public static List<Models.SearchResult> Query(string text, int max = 8, string userId = null)
        {
            if (!string.IsNullOrEmpty(text) && !text.EndsWith(":"))
            {
                userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
                var analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);    //todo: what version?
                var parser = new Lucene.Net.QueryParsers.QueryParser(Lucene.Net.Util.Version.LUCENE_29, "text", analyzer);  //text is just the default field to search
                var query = parser.Parse(text);
                //var term = new Term("text", text.ToLower());
                //var query = new Lucene.Net.Search.PrefixQuery(term); //parser.Parse(text);
                //var query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "text", analyzer).Parse(text.ToLower());

                using (var dir = FSDirectory.Open(new DirectoryInfo(IndexDir)))
                {
                    using (var searcher = new IndexSearcher(dir, true))
                    {
                        var collector = TopScoreDocCollector.create(10000, true); //todo: mini-hack to accomidate the post-filtering of search results - pull a lot of records, then filter out only top (max)
                        searcher.Search(query, collector);
                        var hits = collector.TopDocs().ScoreDocs;

                        //var hits = searcher.Search(query);
                        var ret = new List<Models.SearchResult>();

                        for (var i = 0; i < hits.Length; i++)
                        {
                            var docId = hits[i].doc;
                            var doc = new Models.SearchDocument(searcher.Doc(docId));
                            var provider = GetDocumentProvider(doc.Type);
                            if (provider != null)
                            {
                                if (provider.IsAuthorized(doc, userId))
                                    ret.Add(provider.FormatResult(doc));
                            }
                            else
                                throw new Exception(string.Format("Formatter for type {0} not found", doc.Type));
                            
                            if (ret.Count >= max)   //todo: mini-hack to accomidate the post-filtering of search results
                                break;
                        }

                        return ret;
                    }
                }
            }
            return new List<SearchResult>();
        }

        public static List<string> Generate(string id, string userId = null, bool isRetry = false)
        {
            var ret = new List<string>();
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var provider = GetSearchProviderById(id);
            if (provider != null)
            {
                var analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
                var dirInfo = new DirectoryInfo(IndexDir);
                using (var indexFSDir = FSDirectory.Open(dirInfo, new Lucene.Net.Store.SimpleFSLockFactory(dirInfo)))
                {
                    try 
                    {
                        using (var writer = new Lucene.Net.Index.IndexWriter(indexFSDir, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
                        {
                            var service = provider.GetService();
                            ret.AddRange(service.Generate(writer));
                            provider.LastGenerated = DateTime.UtcNow;
                            Save(provider, userId);
                        }
                    }
                    catch (LockObtainFailedException ex)
                    {
                        Logging.Logger.InfoFormat("LockObtainFailedException {0} - trying to release lock", ex.Message);
                        IndexWriter.Unlock(indexFSDir);
                        if (!isRetry)
                            Generate(id, userId, true);
                    }
                }
            }
            return ret;
        }

        public static void ClearIndex(string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var directoryInfo = new DirectoryInfo(IndexDir);
            Parallel.ForEach(directoryInfo.GetFiles(), file =>
            {
                file.Delete();
            });
            var providers = GetSearchProviders();
            foreach (var provider in providers)
            {
                provider.LastGenerated = null;
                Save(provider, userId);
            }
        }

        public static List<Models.SearchProvider> GetSearchProviders()
        {
            return Repository.GetResources<Models.SearchProvider>("SearchProvider").Select(m => m.Data).OrderBy(i => i.Name).ToList();
        }
        public static Models.SearchProvider GetSearchProvider(string name)
        {
            return GetSearchProviders().Where(m => m.Name == name).FirstOrDefault();
        }
        public static Models.SearchProvider GetSearchProviderById(string Id)
        {
            return GetSearchProviders().Where(m => m.Id == Id).FirstOrDefault();
        }
        public static string Import(Models.SearchProvider provider, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = GetSearchProvider(provider.Name);
            provider.Id = existing != null ? existing.Id : null;
            return Save(provider, userId);
        }
        public static string Save(Models.SearchProvider provider, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            if (!IsDuplicate(provider))
            {
                var res = Repository.StoreResource("SearchProvider", null, provider, userId);
                return res.Id;
            }
            else
                throw new Exception(string.Format(Localization.GetLocalization(LocalizationType.Exception, "DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "Core"), "Widget Manifest"));
        }
        public static bool IsDuplicate(Models.SearchProvider provider)
        {
            var m = GetSearchProviderById(provider.Name);
            if (m != null)
                return m.Id != provider.Id;
            return false;
        }
        public static bool Exists(Models.SearchProvider provider)
        {
            var m = GetSearchProvider(provider.Name);
            return (m != null);
        }
        public static bool DeleteSearchProvider(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var res = Repository.GetResourceById<Models.SearchProvider>(id);
            if (res != null)
                Repository.Delete(res);
            return res != null;
        }

        public static void RegisterForAutoUpdate()
        {
            CacheTimer.Elapsed += OnAutoUpdate;
        }

        private static void OnAutoUpdate(object sender, EventArgs args)
        {
            Services.Logging.Logger.Debug("Search.OnAutoUpdate");
            var providers = GetSearchProviders().Where(p => p.AutoRefreshRate.HasValue && (!p.LastGenerated.HasValue || p.LastGenerated.Value.AddMinutes(p.AutoRefreshRate.Value) < DateTime.UtcNow));
            foreach (var provider in providers)
                Generate(provider.Id);

        }
    }
}
