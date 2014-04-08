using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Videre.Core.Models
{
    public class CustomDataElement
    {
        public string Name { get; set; }
        public bool Required { get; set; }
        public bool UserCanEdit { get; set; }
        public Type DataType { get; set; }
        public List<SelectListItem> Values { get; set; }
        public string DefaultValue { get; set; }

        public string SafeName
        {
            get
            {
                return Name.Replace(" ", "-");
            }
        }


    }
}
