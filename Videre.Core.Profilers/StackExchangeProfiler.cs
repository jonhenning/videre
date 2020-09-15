using StackExchange.Profiling;
using StackExchange.Profiling.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Videre.Core.Providers;

namespace Videre.Core.Profilers
{
    public class StackExchangeProfiler : IVidereHttpApplication
    {
        private bool? _isEnabled = null;
        private string _routePrefix = null;
        private bool isEnabled
        {
            get
            {
                if (!_isEnabled.HasValue)
                    _isEnabled = Videre.Core.Services.Portal.GetAppSetting("Profiler", "") == "StackExchange";
                return _isEnabled ?? false;
            }
        }

        private string routePrefix
        {
            get
            {
                if (_routePrefix == null)
                    _routePrefix = Videre.Core.Services.Portal.GetAppSetting("MiniProfilerRoutePrefix", "_profiler");
                return _routePrefix;
            }
        }

        public static void AddHttpModules()
        {
        }

        public void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        public void Application_BeginRequest(object sender, EventArgs e)
        {
            if (isEnabled)
            {
                // You can decide whether to profile here, or it can be done in ActionFilters, etc.
                // We're doing it here so profiling happens ASAP to account for as much time as possible.
                //if (Request.IsLocal) // Example of conditional profiling, you could just call MiniProfiler.StartNew();
                //{
                    MiniProfiler.Start();
                //}
            }
        }

        public void Application_End()
        {
        }

        public void Application_EndRequest(object sender, EventArgs e)
        {
            if (isEnabled)
            {
                MiniProfiler.Stop(discardResults: false);   // Be sure to stop the profiler!
            }
        }

        public void Application_Error(object sender, EventArgs e)
        {
        }

        public void Application_Start()
        {
            if (isEnabled)
            {


                //MiniProfiler.Configure(new MiniProfilerOptions()
                //    {
                // Sets up the route to use for MiniProfiler resources:
                // Here, ~/profiler is used for things like /profiler/mini-profiler-includes.js
                MiniProfiler.Settings.RouteBasePath = "~/" + routePrefix; //"~/_profiler",

                        // Example of using SQLite storage instead
                        //Storage = new SqliteMiniProfilerStorage(ConnectionString),

                        // Different RDBMS have different ways of declaring sql parameters - SQLite can understand inline sql parameters just fine.
                        // By default, sql parameters will be displayed.
                        //SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter(),

                        // These settings are optional and all have defaults, any matching setting specified in .RenderIncludes() will
                        // override the application-wide defaults specified here, for example if you had both:
                        //    PopupRenderPosition = RenderPosition.Right;
                        //    and in the page:
                        //    @MiniProfiler.Current.RenderIncludes(position: RenderPosition.Left)
                        // ...then the position would be on the left on that page, and on the right (the application default) for anywhere that doesn't
                        // specified position in the .RenderIncludes() call.
                        //PopupRenderPosition = RenderPosition.Right,  // defaults to left
                        //PopupMaxTracesToShow = 10,                   // defaults to 15
                        //ColorScheme = ColorScheme.Auto,              // defaults to light

                        // ResultsAuthorize (optional - open to all by default):
                        // because profiler results can contain sensitive data (e.g. sql queries with parameter values displayed), we
                        // can define a function that will authorize clients to see the JSON or full page results.
                        // we use it on http://stackoverflow.com to check that the request cookies belong to a valid developer.
                        //ResultsAuthorize = request => request.IsLocal,

                        // ResultsListAuthorize (optional - open to all by default)
                        // the list of all sessions in the store is restricted by default, you must return true to allow it
                        //ResultsListAuthorize = request =>
                        //{
                            // you may implement this if you need to restrict visibility of profiling lists on a per request basis
                        //    return true; // all requests are legit in this example
                        //},

                        // Stack trace settings
                        //StackMaxLength = 256, // default is 120 characters

                        // (Optional) You can disable "Connection Open()", "Connection Close()" (and async variant) tracking.
                        // (defaults to true, and connection opening/closing is tracked)
                        //TrackConnectionOpenClose = true
                //    })
                // Optional settings to control the stack trace output in the details pane, examples:
                //.ExcludeType("SessionFactory")  // Ignore any class with the name of SessionFactory)
                //.ExcludeAssembly("NHibernate")  // Ignore any assembly named NHibernate
                //.ExcludeMethod("Flush")         // Ignore any method with the name of Flush
                //.AddViewProfiling()              // Add MVC view profiling (you want this)
                                                     // If using EntityFrameworkCore, here's where it'd go.
                                                     // .AddEntityFramework()        // Extension method in the MiniProfiler.EntityFrameworkCore package
                ;

                // If we're using EntityFramework 6, here's where it'd go.
                // This is in the MiniProfiler.EF6 NuGet package.
                // MiniProfilerEF6.Initialize();

                MiniProfiler.Settings.Results_Authorize = request => {
                    return Videre.Core.Services.Authentication.IsAuthenticated && Videre.Core.Services.Account.CurrentUser.IsActivityAuthorized("Profiler", "MiniProfiler");
                };
                MiniProfiler.Settings.Results_List_Authorize = request => {
                    return Videre.Core.Services.Authentication.IsAuthenticated && Videre.Core.Services.Account.CurrentUser.IsActivityAuthorized("Profiler", "MiniProfiler");
                };

                MiniProfiler.Settings.PopupStartHidden = Videre.Core.Services.Portal.GetAppSetting("MiniProfilerStartHidden", false);
                MiniProfiler.Settings.PopupRenderPosition = RenderPosition.BottomRight;

            }
        }

        public void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            if (isEnabled)
            {
                filters.Add(new ProfilingActionFilter());
            }
        }

        public void RegisterRoutes(RouteCollection routes)
        {
            if (isEnabled)
            {
                MiniProfilerHandler.RegisterRoutes();
                routes.IgnoreRoute(routePrefix + "/{*pathInfo}");
            }
        }
    }
}