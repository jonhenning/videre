using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace Videre.Core.Providers
{
    public interface IVidereHttpApplication
    {
        void Application_Start();
        void Application_BeginRequest(object sender, EventArgs e);
        void Application_AuthenticateRequest(object sender, EventArgs e);
        void Application_End();
        void Application_Error(object sender, EventArgs e);
        void Application_EndRequest(object sender, EventArgs e);
        void RegisterGlobalFilters(GlobalFilterCollection filters);
        void RegisterRoutes(RouteCollection routes);

    }
}
