using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbersArmy.Core.Models
{
    public class OCRLine
    {
        public String Text { get; set; }
        public IReadOnlyList<OCRWord> Words { get; set; }
    }
}
