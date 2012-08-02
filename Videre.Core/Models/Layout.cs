using System.Collections.Generic;
using CodeEndeavors.Extensions;

namespace Videre.Core.Models
{
    public class Layout
    {
        public string Name { get; set; }
        public List<List<LayoutPane>> Panes { get; set; }
    }
}
