using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.Models
{
    public class ImportExportContent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Preview { get; set; }
        public bool Included { get; set; }
        public string Type { get; set; }
    }
}
