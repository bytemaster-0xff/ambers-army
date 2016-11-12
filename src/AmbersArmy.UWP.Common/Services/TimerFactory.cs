using AmbersArmy.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbersArmy.UWP.Common.Services
{
    public class TimerFactory : ITimerFactory
    {
        public ITimer CreateTimer(TimeSpan interval)
        {
            return new Timer(interval);
        }
    }
}
