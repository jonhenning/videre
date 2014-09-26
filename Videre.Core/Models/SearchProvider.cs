using System;
using System.Collections.Generic;
using CodeEndeavors.Extensions;

namespace Videre.Core.Models
{
    public class SearchProvider
    {
        public SearchProvider()
        {
            DocumentTypes = new List<string>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string ProviderType { get; set; }
        public List<string> DocumentTypes { get; set; }

        public DateTimeOffset? LastGenerated { get; set; }
        public int? AutoRefreshRate { get; set; }

        public Services.ISearchProvider GetService()
        {
            return Videre.Core.Services.Search.GetService(Name);
        }

    }

}
