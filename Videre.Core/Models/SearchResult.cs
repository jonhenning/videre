using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;

namespace Videre.Core.Models
{
    public class SearchResult
    {
        public string Id {get;set;}
        public string Type {get;set;}
        public string Name { get; set; }
        public string Summary {get;set;}
        public string Url {get;set;}
    }
}
