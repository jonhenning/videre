using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheClient = CodeEndeavors.Distributed.Cache.Client;
using CodeEndeavors.Extensions;

namespace Videre.Core.Services
{
    public class Caching
    {
        private static bool _initialized = false;
        public static void Initialize() //todo: keep this lazy load?
        {
            if (!_initialized)
            {
                CacheClient.Service.RegisterCache("VidereRequestCache", "{cacheType: 'CodeEndeavors.Distributed.Cache.Client.Web.HttpRequestCache'}");
                CacheClient.Service.RegisterCache("VidereFileCache", "{cacheType: 'CodeEndeavors.Distributed.Cache.Client.InMemory.InMemoryCache'}");

                CacheClient.Service.RegisterCache("VidereWidgetCache", Portal.GetAppSetting("WidgetCacheConnection", "{cacheType: 'CodeEndeavors.Distributed.Cache.Client.InMemory.InMemoryCache'}"));
                _initialized = true;
            }
        }

        public static T GetRequestCacheEntry<T>(string cacheKey, Func<T> lookupFunc)
        {
            Initialize();
            return CacheClient.Service.GetCacheEntry("VidereRequestCache", cacheKey, lookupFunc);
        }

        public static bool RemoveRequestCacheEntry(string cacheKey)
        {
            Initialize();
            return CacheClient.Service.RemoveCacheEntry("VidereRequestCache", cacheKey);
        }

        public static T GetFileJSONObject<T>(string fileName)
        {
            return CacheClient.Service.GetCacheEntry<T>("VidereFileCache", fileName, () =>
                {
                    var json = fileName.GetFileContents();
                    return json.ToObject<T>();
                }, new { monitorType = "CodeEndeavors.Distributed.Cache.Client.File.FileMonitor", fileName = fileName, uniqueProperty = "fileName" });
        }

    }
}
