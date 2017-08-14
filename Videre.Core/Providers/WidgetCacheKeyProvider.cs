using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Videre.Core.Providers
{
    public class WidgetCacheKeyProvider : IWidgetCacheKeyProvider
    {
        public string Name { get { return "Widget"; } }
        public string GetVaryByCustomString(Models.Widget widget)
        {
            return widget.Id;
        }
    }
}
