using System;
using CodeEndeavors.Extensions;
using System.Configuration;
using System.Web;
using System.Web.Caching;

namespace Videre.Core.Services
{
    //TODO: poor mans timer - possibly integrate quartz or some other job scheduler...
    public class CacheTimer
    {
        public static event EventHandler Elapsed;

        public static void Register()
        {
            Services.Logging.Logger.Debug("Cachetimer.Register");
            var minutes = ConfigurationManager.AppSettings.GetSetting("CacheTimerMinutes", 1);
            HttpRuntime.Cache.Add("_cacheTimer", "", null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(minutes), CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(OnElapsed));
        }

        private static void OnElapsed(string key, object value, CacheItemRemovedReason reason)
        {
            Services.Logging.Logger.Debug("Cachetimer.OnElapsed");
            Elapsed.Invoke(null, new EventArgs());
            Register();
        }

    }
}
