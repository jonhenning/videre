using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Extensions;
using CodeEndeavors.Extensions;

namespace Videre.Core.Services
{
    public class TokenParser
    {
        private static List<Models.TokenParserRule> _tokenRules = new List<Models.TokenParserRule>()
        {
            {new Models.TokenParserRule() { 
                Token = "BASEURL", 
                TokenRule = (string text, Models.TokenParserRule rule) => 
                {
                    var baseUrl = Videre.Core.Services.Portal.ResolveUrl("~/");
                    var absoluteUrl = Videre.Core.Services.Portal.RequestRootUrl.PathCombine("", "/");
                    //todo:  regex probably better here!
                    text = text.Replace(string.Format(" href=\"{0}", baseUrl), " href=\"" + GetTokenText(rule.Token));
                    text = text.Replace(string.Format(" src=\"{0}", baseUrl), " src=\"" + GetTokenText(rule.Token));

                    text = text.Replace(string.Format(" href=\"{0}", absoluteUrl), " href=\"" + GetTokenText(rule.Token));
                    text = text.Replace(string.Format(" src=\"{0}", absoluteUrl), " src=\"" + GetTokenText(rule.Token));
                    return text;
                },
                DetokenRule = (string text, Models.TokenParserRule rule) => 
                {
                    var baseUrl = Videre.Core.Services.Portal.ResolveUrl("~/");
                    //todo:  regex probably better here!
                    return text.Replace(GetTokenText(rule.Token), baseUrl); 
                }
            }},
            {new Models.TokenParserRule() { 
                Token = "VIDERE_VERSION", 
                TokenRule = null,
                DetokenRule = (string text, Models.TokenParserRule rule) => 
                {
                    var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    //todo:  regex probably better here!
                    return text.Replace(GetTokenText(rule.Token), version); 
                }
            }},
            {new Models.TokenParserRule() { 
                Token = "VIDERE_PORTAL_TITLE", 
                TokenRule = null,
                DetokenRule = (string text, Models.TokenParserRule rule) => 
                {
                    var title = Services.Portal.CurrentPortal.Title;
                    //todo:  regex probably better here!
                    return text.Replace(GetTokenText(rule.Token), title); 
                }
            }}

        };

        public static string GetTokenText(string token)
        {
            return string.Format("[[[{0}]]]", token);
        }

        public static Models.TokenParserRule GetRule(string token)
        {
            return _tokenRules.Where(r => r.Token == token).FirstOrDefault();
        }

        public static void RegisterTokenRule(Models.TokenParserRule rule)
        {
            if (!_tokenRules.Exists(r => r.Token == rule.Token))
                _tokenRules.Add(rule);
        }

        /// <summary>
        /// Replace tokenized content with their replacement text
        /// Rules that have both a TokenRule and DetokenRule will be processed here.  
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ReplaceTokensWithContent(string value)
        {
            foreach (var rule in _tokenRules.Where(r => r.DetokenRule != null && r.TokenRule != null && value.IndexOf(GetTokenText(r.Token)) > -1).OrderBy(r => r.Priority))
                value = rule.DetokenRule(value, rule);
            return value;
        }

        /// <summary>
        /// Replace content with tokenized content
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ReplaceContentWithTokens(string value)
        {
            foreach (var rule in _tokenRules.Where(r => r.TokenRule != null).OrderBy(r => r.Priority))
                value = rule.TokenRule(value, rule);
            return value;
        }

    }
}
