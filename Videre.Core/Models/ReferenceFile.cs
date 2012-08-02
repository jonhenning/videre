using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.Models
{
    public enum ReferenceFileType
    {
        Css = 0,
        Script = 1,
        Image = 2
    }

    public class ReferenceFile
    {
        public string Path { get; set; }
        public ReferenceFileType Type { get; set; }
    }
}
