using System.Collections.Generic;

namespace Videre.Core.Extensions.Bootstrap
{
    public class BootstrapBaseControlModel
    {
        public string id {get;set;}
        public string html { get; set; }
        public string dataColumn { get; set; }
        public string title { get; set; }
        public IDictionary<string, object> htmlAttributes = new Dictionary<string, object>();
        public List<string> CssClasses = new List<string>();
    }
}
