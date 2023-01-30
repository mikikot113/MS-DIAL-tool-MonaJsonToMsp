using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonaJsonToMsp.Models
{
    public class MainWindowModel
    {
        public string? JsonFolder { get; set; } = string.Empty;
        public string? ExportFolder { get; set; } = string.Empty;
        public string? OntologyFile { get; set; } = string.Empty;
        public bool emptyIsPos { get; set; } = false;
        public bool posNegSame { get; set; } = false;
    }
}
