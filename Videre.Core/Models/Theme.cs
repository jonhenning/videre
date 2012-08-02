using System.Collections.Generic;
using CodeEndeavors.Extensions;

namespace Videre.Core.Models
{
    public class Theme
    {
        public Theme()
        {
            Files = new List<ReferenceFile>();
        }

        //public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public string Thumbnail { get; set; }
        public List<ReferenceFile> Files { get; set; }
    }

}
