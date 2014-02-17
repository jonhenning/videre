using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CodeEndeavors.Extensions;
using CodeEndeavors.Extensions.Serialization;

namespace Videre.Core.Models
{
    public class Portal
    {

        public Portal()
        {
            //AttributeDefinitions = new Dictionary<string, List<AttributeDefinition>>();
            Attributes = new Dictionary<string, object>();
            Aliases = new List<string>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string LogoUrl { get; set; }
        public string ThemeName { get; set; }
        public bool Default { get; set; }
        public List<string> Aliases {get;set;}
        //public string ThemeCss { get; set; }

        //[SerializeIgnore(new string[] { "db", "client" })]
        //public Dictionary<string, List<Models.AttributeDefinition>> AttributeDefinitions { get; set;}
        public Dictionary<string, object> Attributes { get; set; }

        public T GetAttribute<T>(string groupName, string name, T defaultValue)
        {
            object value;
            if (Attributes.TryGetValue(GetAttributeKey(groupName, name), out value))
                return value.ToType<T>();

            if (Services.Portal.AttributeDefinitions.ContainsKey(groupName))
            {
                var attribute = Services.Portal.AttributeDefinitions[groupName].Where(a => a.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                if (attribute != null)
                    return attribute.DefaultValue.ToType<T>();
            }
            return defaultValue;
        }

        public void SetAttribute<T>(string groupName, string name, T value)
        {
            var key = GetAttributeKey(groupName, name);
            Attributes[key] = value;
        }

        public string GetAttributeKey(string groupName, string name)
        {
            return string.Format("{0}.{1}", groupName, name);
        }

    }
}
