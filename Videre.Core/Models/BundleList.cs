using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.Models
{
    public class BundleList
    {
        public BundleList()
        {
            Items = new List<Models.ReferenceListItem>();
        }
        public List<Models.ReferenceListItem> Items { get; set; }
        public bool Bundle { get; set; }
    }
}
