//using CodeEndeavors.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Videre.Core.RouteParserProviders
{
    public class VidereRouteParser : IRouteParser 
    {
        private Dictionary<string, string> _parseTypes = new Dictionary<string, string>()
        {
            {"int", @"[0-9]+"},
            {"string", @"[\w\ \.\/\-\(\)]+"}
        };

        private ConcurrentDictionary<string, Regex> _compiledRegexes = new ConcurrentDictionary<string, Regex>();

        public void RegisterParseType(string type, string pattern)
        {
            _parseTypes[type] = pattern;
            _compiledRegexes = new ConcurrentDictionary<string, Regex>(); //reset cache - simple to just do all
        }

        public Dictionary<string, string> Parse(string pattern, string url)
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

        public List<Models.RouteSegment> GetSegments(string pattern)
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

        public string GetBestMatchedUrl(string url, IEnumerable<string> urls)
        {
            var queries = new List<Query<string>>
            {
                new Query<string>(u => Parse(u, url).Keys.Count > 0, 1)
            };

            var matchedUrls = GetMatches(queries, urls, false);
            if (matchedUrls.Count > 0)
            {
                //our most specific match is determined by number of matching groups from regex... - if tie then use length of matchedUrl
                return (from u in matchedUrls
                        orderby Parse(u, url).Keys.Count descending, u.Length descending
                        select u).FirstOrDefault();
            }
            return null;
        }

        //taken out of CodeEndeavors.ResourceManager
        private class Query<T>
        {
            public Query()
            {

            }
            public Query(Func<T, dynamic> Statement, int Score)
            {
                this.Statement = Statement;
                this.Score = Score;
            }

            public Func<T, dynamic> Statement { get; set; }
            public int Score { get; set; }
        }

        private List<T> GetMatches<T>(List<Query<T>> Queries, IEnumerable<T> Items, bool BestMatch)
        {
            var matchedItems = new List<T>();
            //todo: perf... double match score calculation
            var matches = Items.Where(i => GetMatchScore(Queries, i) > 0)
                    .OrderByDescending(i => GetMatchScore(Queries, i))
                    .ToList();
            if (matches.Count > 0)
            {
                if (BestMatch)
                    matchedItems.Add(matches[0]);
                else
                    matchedItems.AddRange(matches);
            }
            return matchedItems;
        }

        private int GetMatchScore<T>(List<Query<T>> Queries, T scopeObject)
        {
            var score = 0;
            foreach (var q in Queries)
            {
                //itemScore = 0;
                try
                {
                    //if (scopeObject.Where(q.Statement).Count() > 0)
                    if (q.Statement(scopeObject))
                        score += q.Score;
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException ex)
                {
                    //ignore
                }
            }
            return score;
        }

    }
}
