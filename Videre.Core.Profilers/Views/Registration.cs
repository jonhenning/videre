using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Videre.Core.Models;
using Videre.Core.Services;

namespace Videre.Core.Profilers.Views
{
    public class Registration : IWidgetRegistration
    {
        public int Register()
        {
            int updates = 0;
            updates += Update.Register(new List<Models.WidgetManifest>()
            {
                new Models.WidgetManifest() { Path = "Core/Profilers", Name = "MiniProfilerScripts", Title = "Mini Profiler Scripts", Category = "Profilers" },
            });
            return updates;
        }

        public int RegisterPortal(string portalId)
        {
            int updates = 0;
            updates += Update.Register(new List<Models.SecureActivity>()
            {
                new Models.SecureActivity() { PortalId = portalId, Area = "Profiler", Name = "MiniProfiler", RoleIds = new List<string>() {Update.GetAdminRoleId(portalId)} }
            });

            return updates;
        }
    }
}