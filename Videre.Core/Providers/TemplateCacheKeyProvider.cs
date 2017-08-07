using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Videre.Core.Providers
{
    public class TemplateCacheKeyProvider : IWidgetCacheKeyProvider
    {
        public string Name { get { return "Template"; } }
        public string GetVaryByCustomString(Models.Widget widget)
        {
            if (Core.Services.Portal.CurrentTemplate != null)
                return Core.Services.Portal.CurrentTemplate.Id;
            return null;
        }
    }
}
