using System.Collections.Generic;

namespace Videre.Core.Models
{
    public class AttributeDefinition
    {
        public AttributeDefinition()
        {
            Values = new List<string>();
            Dependencies = new List<AttributeDependency>();
        }

        public string LabelKey { get; set; }
        public string LabelText { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }
        public string ControlType { get; set; }
        public string InputType { get; set; }
        public bool Required { get; set; } //todo!

        public List<string> Values { get; set; } //todo: if values is set then dropdown, otherwise textbox???
        public object DefaultValue { get; set; }
        public List<AttributeDependency> Dependencies { get; set; }
    }
}