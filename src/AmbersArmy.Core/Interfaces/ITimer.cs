using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbersArmy.Core.Interfaces
{
    public interface ITimer
    {
        event EventHandler Elapsed;
        TimeSpan Interval { get; set; }
        void Start();
        void Stop();
    }
}
