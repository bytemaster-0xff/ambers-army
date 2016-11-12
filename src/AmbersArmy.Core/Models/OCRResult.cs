using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbersArmy.Core.Models
{
    public class OCRResult
    {
        public String AllText { get; set; }
        public IReadOnlyList<OCRLine> Lines { get; set; }
    }
}
