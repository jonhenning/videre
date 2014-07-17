using CodeEndeavors.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Videre.Core.Providers;

namespace Videre.Core.Services
{
    public class RouteParser
    {
        private static IRouteParser _routeParser;

        public static IRouteParser RouteParserInstance
        {
            get
            {
                if (_routeParser == null)
                    _routeParser = Portal.GetAppSetting("RouteParserProvider", "Videre.Core.Providers.VidereRouteParser, Videre.Core").GetInstance<IRouteParser>();
                return _routeParser;
            }
        }

        public static void RegisterParseType(string type, string pattern)
        {
            RouteParserInstance.RegisterParseType(type, pattern);
        }

        public static Dictionary<string, string> Parse(string pattern, string url)
        {
            return RouteParserInstance.Parse(pattern, url);
        }

        public static List<Models.RouteSegment> GetSegments(string pattern)
        {
            return RouteParserInstance.GetSegments(pattern);
        }

        public static string GetBestMatchedUrl(string url, IEnumerable<string> urls)
        {
            return RouteParserInstance.GetBestMatchedUrl(url, urls);
        }
    }
}
