using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.RouteParserProviders
{
    public interface IRouteParser
    {
        void RegisterParseType(string type, string pattern);
        Dictionary<string, string> Parse(string pattern, string url);
        List<Models.RouteSegment> GetSegments(string pattern);
        string GetBestMatchedUrl(string url, IEnumerable<string> urls);
    }
}
