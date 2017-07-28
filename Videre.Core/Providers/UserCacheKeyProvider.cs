using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Videre.Core.Providers
{
    public class UserCacheKeyProvider : IWidgetCacheKeyProvider
    {
        public string Name { get { return "User"; } }
        public string GetVaryByCustomString(Models.Widget widget)
        {
            if (Videre.Core.Services.Authentication.IsAuthenticated)
                return Videre.Core.Services.Authentication.AuthenticatedUserId;
            return null;
        }
    }
}
