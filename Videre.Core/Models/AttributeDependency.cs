using System.Collections.Generic;
using CodeEndeavors.Extensions;

namespace Videre.Core.Models
{
    public class AttributeDependency
    {
        public AttributeDependency()
        {
            Values = new List<string>();
        }

        public string DependencyName { get; set; }
        public bool? HasValue { get; set; } //if dependency has any value
        public List<string> Values { get; set; }    //if dependency has one of these values
    }

}
