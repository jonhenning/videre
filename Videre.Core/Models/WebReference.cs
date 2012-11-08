using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.Models
{
    public enum WebReferenceLoadType
    {
        Defer = 0,
        Startup = 1,
        EndStartup = 2,
        Inline = 3
    }

    public enum WebReferenceType
    {
        ScriptReference = 0,
        Script = 1,
        StyleSheetReference = 2,
        StyleSheet = 3
    }

    public class WebReference
    {

        public WebReference()
        {
            DependencyGroups = new List<string>();
        }

        public string Id { get; set; }
        public string PortalId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Text { get; set; }
        public int? Sequence { get; set; }
        public WebReferenceType Type { get; set; }
        public WebReferenceLoadType LoadType { get; set; }
        //public bool ForTheme { get; set; }  //todo: bad name?
        public string Group { get; set; }
        public List<string> DependencyGroups { get; set; }
    }
}
