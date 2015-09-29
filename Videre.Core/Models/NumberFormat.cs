using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Videre.Core.Models
{
    public class NumberFormat
    {
        public int CurrencyDecimalDigits { get; set; }
        public string CurrencyDecimalSeparator { get; set; }
        public string CurrencyGroupSeparator { get; set; }
        public string CurrencySymbol { get; set; }
        //public int NumberDecimalDigits { get; set; }
        //public string NumberDecimalSeparator { get; set; }
        //public string NumberGroupSeparator { get; set; }
    }
}
