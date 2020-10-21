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
using Videre.Core.Providers;
using System.Collections.Concurrent;
using CodeEndeavors.ServiceHost.Common.Services.Profiler;

namespace Videre.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static List<IVidereHttpApplication> _applicationPlugins = null;
        private static List<IVidereHttpApplication> applicationPlugins
        {
            get
            {
                if (_applicationPlugins == null)
                    _applicationPlugins = ReflectionExtensions.GetAllInstances<IVidereHttpApplication>();
                return _applicationPlugins;
            }
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            applicationPlugins.ForEach(a => a.RegisterGlobalFilters(filters));
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("scripts/{*pathInfo}");
            routes.IgnoreRoute("content/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");  //todo: eventually allow portal to map this... for now ignore!

            routes.MapRoute(
                "_memorytimings", // Route name
                "_memorytimings/{action}/{key}", // URL with parameters
                new { controller = "MemoryTimer", action = "", key = UrlParameter.Optional },
                namespaces: new string[] { "Videre.Web.Controllers" }
            );

            routes.MapRoute(
                "ServerJS", // Route name
                "ServerJS/{action}/{key}", // URL with parameters
                new { controller = "ServerJS", action = "", key = UrlParameter.Optional },
                namespaces: new string[] { "Videre.Web.Controllers" }
            );

            routes.MapRoute(
                "Installer", // Route name
                "Installer/{action}/{key}", // URL with parameters
                new { controller = "Installer", action = "", key = UrlParameter.Optional },
                namespaces: new string[] { "Videre.Web.Controllers" }
            );

            applicationPlugins.ForEach(a => a.RegisterRoutes(routes));

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
            try
            {
                using (var timer = new MemoryTimer("Application_Start"))
                {
                    using (var timing = new MemoryTiming(timer, "ViewEngines"))
                    {
                        //only support Razor for now - perf
                        //https://blogs.msdn.microsoft.com/marcinon/2011/08/16/optimizing-asp-net-mvc-view-lookup-performance/
                        ViewEngines.Engines.Clear();
                        var ve = new RazorViewEngine();
                        ve.ViewLocationCache = new Videre.Core.Providers.TwoLevelViewCache(ve.ViewLocationCache);
                        ViewEngines.Engines.Add(ve);
                    }

                    using (var timing = new MemoryTiming(timer, "Logging Setup"))
                    {
                        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                        var logLevel = Services.Portal.GetAppSetting("LogLevel", 1);

                        CodeEndeavors.ResourceManager.Logging.LogLevel = (CodeEndeavors.ResourceManager.Logging.LoggingLevel)logLevel;
                        CodeEndeavors.ResourceManager.Logging.OnLoggingMessage += (message) => { Services.Logging.Logger.Info("ResourceManager: " + message); };

                        CodeEndeavors.Distributed.Cache.Client.Logging.LogLevel = (CodeEndeavors.Distributed.Cache.Client.Logging.LoggingLevel)logLevel;
                        CodeEndeavors.Distributed.Cache.Client.Logging.OnLoggingMessage += (message) => { Services.Logging.Logger.Info("Cache: " + message); };

                        Services.Logging.Logger.Debug("Application_Start");
                    }
                    using (var timing = new MemoryTiming(timer, "WatchForUpdates"))
                        Services.Update.WatchForUpdates();
                    using (var timing = new MemoryTiming(timer, "CacheTimer Register"))
                        Services.CacheTimer.Register();
                    using (var timing = new MemoryTiming(timer, "Register AutoUpdate"))
                        Services.Search.RegisterForAutoUpdate();

                    using (var timing = new MemoryTiming(timer, "Servides Update Register"))
                        Services.Update.Register();

                    using (var timing = new MemoryTiming(timer, "Routes, Binders Areas"))
                    {
                        AreaRegistration.RegisterAllAreas();    //todo: not really needed anymore!

                        ModelBinders.Binders.DefaultBinder = new JsonNetModelBinder();

                        RegisterGlobalFilters(GlobalFilters.Filters);
                        RegisterRoutes(RouteTable.Routes);
                    }

                    applicationPlugins.ForEach(a =>
                    {
                        using (var timing = new MemoryTiming(timer, a.GetType().Name + " Plugin Application_Start"))
                            a.Application_Start();
                    });

                    using (var timing = new MemoryTiming(timer, "Repository Dispose"))
                        Core.Services.Repository.Dispose(); //application start is not same httpcontext as first request
                }
            }
            catch (Exception ex)
            {
                Services.Portal.RegisterFailedStartup(ex);
                //Application["ApplicationStartError"] = ex;  //set application state notifying that we failed...  will notify every request (BeginRequest) that our site is not running
                //Services.Logging.Logger.Error("Application_Start", ex);
                //HttpRuntime.UnloadAppDomain();
            }
        }

        public void Application_BeginRequest(object sender, EventArgs e)
        {
            //if (Application["ApplicationStartError"] != null)
            log4net.ThreadContext.Properties["threadid"] = System.Threading.Thread.CurrentThread.ManagedThreadId;   //todo: find a non-specific log4net implementation for this...  

            if (Services.Portal.StartupFailed)
            {
                showErrorPage("[Application_Start]", " ", Services.Portal.ApplicationStartupException);
                Services.Portal.HandleFailedStartup();
            }
            else
                applicationPlugins.ForEach(a => a.Application_BeginRequest(sender, e));
        }

        private void showErrorPage(string controllerName, string actionName, Exception exception)
        {
            var routeData = new RouteData();
            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = "Index";

            Response.StatusCode = 500;

            // Avoid IIS7 getting involved
            Response.TrySkipIisCustomErrors = true;

            // Execute the error controller
            IController errorsController = new Videre.Web.Controllers.ErrorController();
            routeData.Values["exception"] = new HandleErrorInfo(exception, controllerName, actionName);

            var wrapper = new HttpContextWrapper(Context);
            var rc = new RequestContext(wrapper, routeData);
            errorsController.Execute(rc);
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            Services.Authentication.ProcessAuthenticationTicket();
            applicationPlugins.ForEach(a => a.Application_AuthenticateRequest(sender, e));

        }

        protected void SessionAuthenticationModule_SessionSecurityTokenReceived(object sender, System.IdentityModel.Services.SessionSecurityTokenReceivedEventArgs e)
        {
            var token = Services.Authentication.ProcessAuthenticationTicket(e.SessionToken);
            if (token != null)
            {
                e.ReissueCookie = true;
                e.SessionToken = token;
            }
        }

        public void Application_End()
        {
            Services.Update.ApplicationShutdown = true;
            //http://weblogs.asp.net/scottgu/archive/2005/12/14/433194.aspx
            HttpRuntime runtime = (HttpRuntime)typeof(System.Web.HttpRuntime).InvokeMember("_theRuntime", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField, null, null, null);

            if (runtime == null)
                return;

            string shutDownMessage = (string)runtime.GetType().InvokeMember("_shutDownMessage", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime, null);
            string shutDownStack = (string)runtime.GetType().InvokeMember("_shutDownStack", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime, null);
            Services.Logging.Logger.Error(String.Format("\r\n\r\n_shutDownMessage={0}\r\n\r\n_shutDownStack={1}", shutDownMessage, shutDownStack));

            applicationPlugins.ForEach(a => a.Application_End());
            Services.Update.RemoveAppOffline();
        }


        public void Application_Error(object sender, EventArgs e)
        {
            applicationPlugins.ForEach(a => a.Application_Error(sender, e));
            Services.Logging.Logger.Error("Application_Error", this.Server.GetLastError());
        }

        public void Application_EndRequest(object sender, EventArgs e)
        {
            applicationPlugins.ForEach(a => a.Application_EndRequest(sender, e));
            Services.Repository.Dispose();
        }

        private static ConcurrentDictionary<string, System.Reflection.Assembly> _resolvedNames = new ConcurrentDictionary<string, System.Reflection.Assembly>();
        public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            if (name.Name != args.Name)
            {
                if (!_resolvedNames.ContainsKey(name.Name))
                {
                    Services.Logging.Logger.Debug(string.Format("Videre.CurrentDomain_AssemblyResolve: {0} != {1}", name.Name, args.Name));
                    _resolvedNames[name.Name] = System.Reflection.Assembly.LoadWithPartialName(name.Name);
                }
                else
                    Services.Logging.Logger.Debug(string.Format("Videre.CurrentDomain_AssemblyResolve (cached): {0} != {1}", name.Name, args.Name));
                return _resolvedNames[name.Name];
            }
            return null;
        }

    }

}