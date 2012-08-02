using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;

namespace Videre.Core.Services
{
    public interface ISearchProvider
    {
        List<string> Generate(IndexWriter writer);
        bool IsAuthorized(Models.SearchDocument doc, string userId);
        Models.SearchResult FormatResult(Models.SearchDocument doc);

    }
}
