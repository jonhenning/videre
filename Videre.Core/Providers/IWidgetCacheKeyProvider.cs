using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Videre.Core.Providers
{
    public interface IWidgetCacheKeyProvider
    {
        string Name { get; }
        string GetVaryByCustomString(Models.Widget widget);
    }
}
