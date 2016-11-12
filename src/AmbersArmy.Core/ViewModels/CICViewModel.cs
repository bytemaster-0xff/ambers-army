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
    public class CICViewModel
    {
   
        FlowClient _flowClient = new FlowClient();

        public CICViewModel()
        {
            Alert = new Models.Alert()
            {
                Name = "Amber",
                Age = "12",
                Weight = "80",
                AlertType = Models.Alert.AlertTypes.Amber,
                BirthDate = "June 15, 2012",
                LastKnowLocationDateStamp = DateTime.Now.AddMinutes(-15).ToJSONString(),
                LastKnownLocation = new Models.GeoLocation()
                {
                    Latitude = 33.745239,
                    Longitude = -84.390151
                },
                LicensePlate = "ABC123",
                VehicleColor = "White",
                VehicleModel = "Ford Ranger",
                VehicleYear = "2010",
            };

            AddAlertCommand = new RelayCommand(PostAlert);
        }

        public async void PostAlert()
        {
            await _flowClient.PostAlertAsync(Alert);
        }

        public Models.Alert Alert { get; private set; }

        public ICommand AddAlertCommand { get; private set; }
    }
}
