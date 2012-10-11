using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Videre.Core.Services
{
    public class RouteParser
    {
        public static Dictionary<string, string> _parseTypes = new Dictionary<string, string>()
        {
            {"int", @"[0-9]+"},
            {"string", @"[\w\ \.\/\-\(\)]+"}
        };

        public static Dictionary<string, Regex> _compiledRegexes = new Dictionary<string, Regex>();

        public static void RegisterParseType(string type, string pattern)
        {
            _parseTypes[type] = pattern;
            _compiledRegexes = new Dictionary<string, Regex>(); //reset cache - simple to just do all
        }

        public static Dictionary<string, string> Parse(string pattern, string url)
        {
            var ret = new Dictionary<string, string>();
            if (pattern != null)    //todo:  not sure if best to do this check here
            {
                var segments = GetSegments(pattern);
                var regPattern = pattern;
                //for (var i = 0; i < segments.Count; i++)
                foreach (var segment in segments)
                {
                    //var segment = segments[i];
                    //regPattern += (!string.IsNullOrEmpty(regPattern) ? "/" : "") + segment.ToRegEx(i == 0, i == segments.Count - 1);
                    regPattern = regPattern.Replace(segment.Pattern, string.Format("(?<{0}>{1})", segment.Name, _parseTypes[segment.Type]));
                }
                if (!_compiledRegexes.ContainsKey(regPattern))
                    _compiledRegexes[regPattern] = new Regex(regPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var regex = _compiledRegexes[regPattern];
                var match = regex.Match(url);
                if (match.Success)
                {
                    for (var i = 0; i < match.Groups.Count; i++)
                        ret[regex.GroupNameFromNumber(i)] = match.Groups[i].Value;
                }
            }
            return ret;
        }

        public static List<Models.RouteSegment> GetSegments(string pattern)
        {
            var paramRegex = new Regex(@"{(?<name>(\w+)):(?<type>(\w+))}*");
            var ret = new List<Models.RouteSegment>();
            foreach (Match m2 in paramRegex.Matches(pattern))
            {
                var type = m2.Groups["type"].Value;
                if (!_parseTypes.ContainsKey(type))
                    throw new Exception("No regex expression defined for Type: " + type);
                ret.Add(new Models.RouteSegment() { Name = m2.Groups["name"].Value, Type = type, Pattern = m2.Groups[0].Value });
            }
            return ret;
        }

    }
}
