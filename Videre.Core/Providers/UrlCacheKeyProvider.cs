using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Videre.Core.Providers
{
    public class UrlCacheKeyProvider : IWidgetCacheKeyProvider
    {
        public string Name { get { return "Url"; } }
        public string GetVaryByCustomString(Models.Widget widget)
        {
            return Core.Services.Portal.CurrentUrl;
        }
    }
}
