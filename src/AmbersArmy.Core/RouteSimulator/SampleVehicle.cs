using AmbersArmy.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbersArmy.Core.RouteSimulator
{
    public class SampleVehicle 
    {
        int _carIndex;
        int _speed;
        int _currentIndex = 0;
        static Random _generator = new Random(DateTime.Now.Millisecond);

        public event EventHandler LocationChanged;

        private List<SampleGeoPos> _samplePoints = new List<SampleGeoPos>();

        public static SampleVehicle Create(int carIndex)
        {
            var vehicle = new SampleVehicle();
            vehicle._carIndex = carIndex;
            var route = _generator.Next(0, 2);
            var direction = _generator.Next(0, 2);
            var start = _generator.Next(1, 120);
            vehicle._speed = _generator.Next(0, 4);

            List<SampleGeoPos> points = null;
            switch(route)
            {
                case 0: points = Routes.Route1; break;
                case 1: points = Routes.Route2; break;
                case 2: points = Routes.Route3; break;
            }

            if(direction == 1)
            {
                for(var idx = start; idx < points.Count; ++idx)
                {
                    vehicle._samplePoints.Add(points[idx]);
                }
            }
            else
            {
                for(var idx = points.Count - start; idx > 0; --idx )
                {
                    vehicle._samplePoints.Add(points[idx]);
                }
            }

            vehicle._currentIndex = 0;

            return vehicle;
        }

        public SampleGeoPos CurrentLocation
        {
            get { return _samplePoints[_currentIndex ]; }
        }

        public int CarIndex { get { return _carIndex; } }

        public void Update()
        {
            _currentIndex+= _speed;
            if (_currentIndex > _samplePoints.Count - 1)
                _currentIndex = 0;

            LocationChanged?.Invoke(this, null);
        }
    }
}
