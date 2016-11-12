using AmbersArmy.Core.Models;
using AmbersArmy.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbersArmy.Core.ViewModels
{
    public class Spotter
    {
        FlowClient _flowClient = new FlowClient();

        public Spotter()
        {

        }

        public GeoLocation CurrentLocation { get; set; }

        public String SpotterId { get; set; }

        public Task PostLocation()
        {
            var location = new Models.Location()
            {
                DeviceId = SpotterId,
                Type = Location.LocationType.Spotter,
                Geo = CurrentLocation,
                TimeStamp = DateTime.Now.ToJSONString()
            };

            return _flowClient.PostLocationAsync(location);
        }
    }
}
