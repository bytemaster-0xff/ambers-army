using AmbersArmy.Core.Models;
using AmbersArmy.Core.Services;
using AmbersArmy.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AmbersArmy.Core.ViewModels
{
    public class Spotter
    {
        FlowClient _flowClient = new FlowClient();

        public Spotter()
        {
            PostDataCommand = new RelayCommand(PostLocation);
        }

        public GeoLocation CurrentLocation { get; set; }

        public String SpotterId { get; set; }

        public async void PostLocation()
        {
            var location = new Models.Location()
            {
                DeviceId = SpotterId,
                Type = Location.LocationType.Spotter,
                Geo = CurrentLocation,
                TimeStamp = DateTime.Now.ToJSONString()
            };

            await _flowClient.PostLocationAsync(location);
        }

        public ICommand PostDataCommand { get; private set; }
    }
}
