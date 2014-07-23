using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.Models
{
    public class ImportExportContentJob
    {
        public ImportExportContentJob()
        {
            Content = new List<ImportExportContent>();
        }

        public string Id { get; set; }
        public string PortalId { get; set; }
        public string Name { get; set; }
        public Models.Package Package { get; set; }
        public List<Models.ImportExportContent> Content { get; set; }

    }
}
