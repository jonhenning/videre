using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using CodeEndeavors.Extensions;
using Videre.Core.Binders;
using Services = Videre.Core.Services;

namespace Videre.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("scripts/{*pathInfo}");
            routes.IgnoreRoute("content/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");  //todo: eventually allow portal to map this... for now ignore!

            routes.MapRoute(
                "ServerJS", // Route name
                "ServerJS/{action}/{key}", // URL with parameters
                new { controller = "ServerJS", action = "", key = UrlParameter.Optional }
            );

            routes.MapRoute(
                "Installer", // Route name
                "Installer/{action}/{key}", // URL with parameters
                new { controller = "Installer", action = "", key = UrlParameter.Optional }
            );

            //the route controller is the "catch all"
            routes.MapRoute(
                "Route", // Route name
                "{*name}", // URL with parameters
                new { controller = "Route", action = "Index" } // Parameter defaults
            );

            if (Services.Portal.GetAppSetting("EnableRouteDebug", false))
                RouteDebug.RouteDebugger.RewriteRoutesForTesting(RouteTable.Routes);

        }

        protected void Application_Start()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            Services.Logging.Logger.Debug("Application_Start");
            Services.Update.WatchForUpdates();
            Services.CacheTimer.Register();
            Services.Search.RegisterForAutoUpdate();

            Services.Update.Register();
            AreaRegistration.RegisterAllAreas();    //todo: not really needed anymore!

            ModelBinders.Binders.DefaultBinder = new JsonNetModelBinder();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);


            Core.Services.Repository.Dispose(); //application start is not same httpcontext as first request

            //if (Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.IsAvailable) 
            //    System.Diagnostics.Trace.Listeners.Add(new Microsoft.WindowsAzure.Diagnostics.DiagnosticMonitorTraceListener());

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            //' Fires upon attempting to authenticate the use
            if (HttpContext.Current.User != null)
            {
                var identity = (FormsIdentity)HttpContext.Current.User.Identity;
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                    HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(identity, identity.Ticket.UserData.Split(','));

            }
        }

        public void Application_End()
        {
            //Log.Info("Application_End");
            //http://weblogs.asp.net/scottgu/archive/2005/12/14/433194.aspx
            HttpRuntime runtime = (HttpRuntime)typeof(System.Web.HttpRuntime).InvokeMember("_theRuntime", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField, null, null, null);

            if (runtime == null)
                return;

            string shutDownMessage = (string)runtime.GetType().InvokeMember("_shutDownMessage", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime, null);
            string shutDownStack = (string)runtime.GetType().InvokeMember("_shutDownStack", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime, null);

            if (!EventLog.SourceExists(".NET Runtime"))
                EventLog.CreateEventSource(".NET Runtime", "Application");

            EventLog log = new EventLog();
            log.Source = ".NET Runtime";
            log.WriteEntry(String.Format("\r\n\r\n_shutDownMessage={0}\r\n\r\n_shutDownStack={1}", shutDownMessage, shutDownStack), EventLogEntryType.Error);
        }


        public void Application_Error(object sender, EventArgs e)
        {
            //Log.Error("Application_Error", this.Server.GetLastError());
        }

        public void Application_EndRequest(object sender, EventArgs e)
        {
            Services.Repository.Dispose();
        }

        public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            if (assemblyName.Name != args.Name)
                return Assembly.LoadWithPartialName(assemblyName.Name);
            return null;
        }

    }
}