using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.Models
{

    public class TokenParserRule
    {
        public int Priority { get; set; }
        public string Token { get; set; }
        public string TokenText
        {
            get
            {
                return string.Format("[[[{0}]]]", Token);
            }
        }
        public Func<string, TokenParserRule, string> TokenRule { get; set; }
        public Func<string, TokenParserRule, string> DetokenRule { get; set; }

    }
}
