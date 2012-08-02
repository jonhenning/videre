using System.Collections.Generic;

namespace Videre.Core.Models
{
    //public class LayoutManifest
    //{
    //    public List<List<LayoutPane>> Panes { get; set; }   //rows, cols
    //}

    public class LayoutPane
    {
        public string Name { get; set; }
        public string DesignerCss { get; set; }
        public string DesignerStyle { get; set; }
    }

}
