using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.Models
{
    public class ReferenceListItem
    {
        public string Src { get; set; }
        public string Text { get; set; }
        public bool ExcludeFromBundle { get; set; }
        public Dictionary<string, string> DataAttributes { get; set; }
        public bool CanBundle
        {
            get
            {
                return !ExcludeFromBundle && DataAttributes == null;
            }
        }
    }
}
