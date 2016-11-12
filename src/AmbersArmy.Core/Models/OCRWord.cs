using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace AmbersArmy.Core.Models
{
    public class OCRWord
    {
        public String Text { get; set; }
        public Rect BoundingBox { get; set; }
    }
}
