using CodeEndeavors.Extensions.Serialization;
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
        public string Id { get; set; }
        public string LabelKey { get; set; }
        public string LabelText { get; set; }
        [SerializeIgnore("db")]
        public string Label
        {
            get
            {
                return Services.Localization.GetPortalText(LabelKey, LabelText);
            }
        }
        public string TooltipKey { get; set; }
        public string TooltipText { get; set; }
        [SerializeIgnore("db")]
        public string Tooltip
        {
            get
            {
                return Services.Localization.GetPortalText(TooltipKey, TooltipText);
            }
        }
        public string Name { get; set; }
        public string GroupName { get; set; }
        public string DataType { get; set; }
        public string ControlType { get; set; }
        public string InputType { get; set; }
        public bool Required { get; set; } //todo!

        public List<string> Values { get; set; } //todo: if values is set then dropdown, otherwise textbox???
        public object DefaultValue { get; set; }
        public List<AttributeDependency> Dependencies { get; set; }
    }
}