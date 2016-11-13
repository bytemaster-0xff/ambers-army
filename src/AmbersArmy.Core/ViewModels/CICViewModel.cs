using AmbersArmy.Core.Services;
using AmbersArmy.Core.Utils;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace AmbersArmy.Core.ViewModels
{
	public class CICViewModel
	{

		FlowClient _flowClient = new FlowClient();
		M2XService _m2xService = new M2XService(Constants.M2XMasterApiKey);
		public ObservableCollection<Models.FoundPlate> FoundPlates = new ObservableCollection<Models.FoundPlate>();

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
				LicensePlate = "EA375A",
				VehicleColor = "White",
				VehicleModel = "Ford Ranger",
				VehicleYear = "2010",
			};

			AddAlertCommand = new RelayCommand(PostAlert);
			InitializeM2XServices();
		}

		private async void InitializeM2XServices()
		{
			await _m2xService.SetupMqttClient(_client_MqttMsgPublishReceived);
		}

		public async void PostAlert()
		{
			await _flowClient.PostAlertAsync(Alert);
		}

		public Models.Alert Alert { get; private set; }

		public ICommand AddAlertCommand { get; private set; }

		private async void _client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
		{
			//if (!M2XMessagesReceived.ContainsKey(e.Topic))
			//{
			//	M2XMessagesReceived.Add(e.Topic, new List<object> { Encoding.UTF8.GetString(e.Message, 0, e.Message.Length) });
			//}
			//else
			//{
			//	M2XMessagesReceived[e.Topic].Add(Encoding.UTF8.GetString(e.Message, 0, e.Message.Length));
			//}

			if (!string.IsNullOrWhiteSpace(e.Topic) && e.Topic.ToLowerInvariant().Contains("responses"))
			{
				var tempClient = new AmbersArmy.M2XProxies.Operations();
				var data = await tempClient.GetUpdatedStreamData(Constants.M2XMasterApiKey, Constants.M2XKnownDeviceId, "licenseplate");
				foreach (var plate in data)
				{
					var foundplate = new Models.FoundPlate
					{
						DeviceId = plate.DeviceId,
						Plate = plate.Plate,
						TimeStamp = plate.TimeStamp
					};


					if (plate != null && plate.Location != null)
					{
						foundplate.Location = new Models.GeoLocation
						{
							Latitude = plate.Location.Latitude,
							Longitude = plate.Location.Longitude
						};
					}

					FoundPlates.Add(foundplate);
				}
			}
		}
	}
}
