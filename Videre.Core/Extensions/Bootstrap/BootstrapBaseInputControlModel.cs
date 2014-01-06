using System.Collections.Generic;

namespace Videre.Core.Extensions.Bootstrap
{
    public class BootstrapBaseInputControlModel : BootstrapBaseControlModel
    {
        public string val {get;set;}
        public string dataType { get; set; }
        public string controlType { get; set; }
        public string mustMatch { get; set; }
        public bool readOnly { get; set; }
        public bool required { get; set; }
        public int? maxLength { get; set; }
        public List<IBootstrapBaseControl> appendControls { get; set; }
        public List<IBootstrapBaseControl> prependControls { get; set; }

        public Bootstrap.BootstrapUnits.InputSize inputSize { get; set; }
        public bool disableAutoComplete { get; set; }
    }
}
