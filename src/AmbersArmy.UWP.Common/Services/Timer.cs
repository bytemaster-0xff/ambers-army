using AmbersArmy.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace AmbersArmy.UWP.Common.Services
{
    public class Timer : ITimer
    {
        DispatcherTimer _timer;

        public Timer(TimeSpan interval)
        {
            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
            _timer.Interval = interval;
        }

        private void _timer_Tick(object sender, object e)
        {
            Elapsed?.Invoke(this, null);
        }

        public TimeSpan Interval
        {
            get { return _timer.Interval; }
           set { _timer.Interval = value; }
        }

        public event EventHandler Elapsed;

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}
