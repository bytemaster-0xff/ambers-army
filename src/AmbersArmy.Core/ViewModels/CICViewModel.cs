using AmbersArmy.Core.Services;
using AmbersArmy.Core.Utils;
using ATTM2X;
using System;
using System.Collections.Generic;
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
		public Dictionary<string, List<object>> M2XMessagesReceived = new Dictionary<string, List<object>>();

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

		private void _client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
		{
			// keep the responses list from occupying too much memory unnecessarily.
			if (!M2XMessagesReceived.ContainsKey(e.Topic))
			{
				M2XMessagesReceived.Add(e.Topic, new List<object> { Encoding.UTF8.GetString(e.Message, 0, e.Message.Length) });
			}
			else
			{
				M2XMessagesReceived[e.Topic].Add(Encoding.UTF8.GetString(e.Message, 0, e.Message.Length));
			}
		}
	}
}
