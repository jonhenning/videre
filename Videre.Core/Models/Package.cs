using System.Collections.Generic;
using CodeEndeavors.Extensions;

namespace Videre.Core.Models
{
    public class Package
    {
        public Package()
        {
        }

        //public string Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public decimal Version { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public string Thumbnail { get; set; }
    }

}
