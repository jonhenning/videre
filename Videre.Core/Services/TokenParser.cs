using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                        //todo:  regex probably better here!
                        text = text.Replace(string.Format(" href=\"{0}", baseUrl), " href=\"" + rule.TokenText);
                        text = text.Replace(string.Format(" src=\"{0}", baseUrl), " src=\"" + rule.TokenText);
                        return text;
                    },
                    DetokenRule = (string text, Models.TokenParserRule rule) => 
                    {
                        //todo:  regex probably better here!
                        var baseUrl = Videre.Core.Services.Portal.ResolveUrl("~/");
                        return text.Replace(rule.TokenText, baseUrl); 
                    }
                }
            }};

        public static Models.TokenParserRule GetRule(string token)
        {
            return _tokenRules.Where(r => r.Token == token).FirstOrDefault();
        }

        public static void RegisterTokenRule(Models.TokenParserRule rule)
        {
            if (!_tokenRules.Exists(r => r.Token == rule.Token))
                _tokenRules.Add(rule);
        }

        public static string ReplaceTokensWithContent(string value)
        {
            foreach (var rule in _tokenRules.OrderBy(r => r.Priority))
                value = rule.DetokenRule(value, rule);
            return value;
        }

        public static string ReplaceContentWithTokens(string value)
        {
            foreach (var rule in _tokenRules.OrderBy(r => r.Priority))
                value = rule.TokenRule(value, rule);
            return value;
        }

    }
}
