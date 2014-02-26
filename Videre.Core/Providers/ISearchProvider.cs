using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;

//namespace Videre.Core.Providers
//{
//    public interface ISearchProvider
//    {
//        List<string> Generate(IndexWriter writer);
//        bool IsAuthorized(Models.SearchDocument doc, string userId);
//        Models.SearchResult FormatResult(Models.SearchDocument doc);

//    }
//}

//TODO: refactor namespace
namespace Videre.Core.Services
{
    //[Obsolete("Use Videre.Core.Providers.ISearchProvider instead")]
    public interface ISearchProvider //: Videre.Core.Providers.ISearchProvider
    {
        List<string> Generate(IndexWriter writer);
        bool IsAuthorized(Models.SearchDocument doc, string userId);
        Models.SearchResult FormatResult(Models.SearchDocument doc);

    }
}
