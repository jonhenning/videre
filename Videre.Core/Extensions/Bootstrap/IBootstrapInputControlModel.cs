using System.Collections.Generic;

namespace Videre.Core.Extensions.Bootstrap
{
    public class IBootstrapInputControlModel : IBootstrapBaseControlModel
    {
        public string val {get;set;}
        public string dataType { get; set; }
        public string mustMatch { get; set; }
        public bool required { get; set; }
        public IBootstrapBaseControl appendControl { get; set; }
        public IBootstrapBaseControl prependControl { get; set; }

        public Bootstrap.BootstrapUnits.InputSize inputSize { get; set; }

        public bool disableAutoComplete { get; set; }
    }
}
