using AmbersArmy.Core.Interfaces;
using AmbersArmy.Core.Models;
using AmbersArmy.Core.Services;
using AmbersArmy.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ATTM2X;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace AmbersArmy.Core.ViewModels
{
	public class Spotter
	{
		FlowClient _flowClient = new FlowClient();
		Interfaces.ILicensePlateReader _plateReader;
		Interfaces.ITimer _timer;
		M2XService _m2xService;
		private ObservableCollection<String> _recognizedTags;
		public Dictionary<string, List<object>> M2XMessagesReceived = new Dictionary<string, List<object>>();

		public Spotter()
		{
			PostLocationCommand = new RelayCommand(PostLocation);
			PostLicensePlateCommand = new RelayCommand(PostLicensePlate);
			CurrentLocation = new GeoLocation()
			{
				Latitude = 36.99,
				Longitude = -86.0
			};

            _plateReader = AAIOC.Get<Interfaces.ILicensePlateReader>();
			_plateReader.TextRecognized += _plateReader_TextRecognized;
			_recognizedTags = new ObservableCollection<string>();

			_m2xService = new M2XService(Constants.M2XMasterApiKey);
		}

		public async Task InitAsync()
		{
			Debug.WriteLine("INITTING READER");
			await _plateReader.InitAsync();
			_timer = AAIOC.Get<ITimerFactory>().CreateTimer(TimeSpan.FromSeconds(1));
			_timer.Elapsed += _timer_Elapsed;
			_timer.Start();
			Debug.WriteLine("WE INIT SO START TIMER!");
		}

        private async void _plateReader_TextRecognized(object sender, OCRResult e)
        {
            if (String.IsNullOrEmpty(e.AllText))
            {
                Debug.WriteLine("MISSING");
            }
            else
            {
                 var plate =  new Models.FoundPlate()
                    {
                        Plate = e.AllText,
                        DeviceId = SpotterId,
                        TimeStamp = DateTime.Now.ToJSONString(),
                        Location = new GeoLocation() { Latitude = 34.3, Longitude = 33.3 }
                    };

                var jsonPayload = JsonConvert.SerializeObject(plate);

                Debug.WriteLine($"VALID => {e.AllText}");

                var timestamp = DateTime.UtcNow.ToJSONString();//.ToString("yyyy-MM-ddTHH:mm:ssZ");
				var postParms = $"{{ \"values\": [ {{ \"timestamp\":\"{timestamp}\", \"value\": \"{jsonPayload.Replace("\"", "&quot;")}\" }} ] }}";
                await _m2xService.PostStreamValues("6e7bb7923219c6b72728cb1a34d0d5b6", "licenseplate", postParms);
            }
        }

        private async void _timer_Elapsed(object sender, EventArgs e)
        {
            _timer.Stop();
            await _plateReader.ScanNow();
            _timer.Start();
        }

		public GeoLocation CurrentLocation { get; set; }

		public String SpotterId { get; set; }

		public async void PostLocation()
		{
			var jsonPayload = JsonConvert.SerializeObject(new Models.Location()
			{
				DeviceId = SpotterId,
				Type = Location.LocationType.Spotter,
				Geo = CurrentLocation,
				TimeStamp = DateTime.Now.ToJSONString()
			});

			var timestamp = DateTime.UtcNow.ToJSONString();//.ToString("yyyy-MM-ddTHH:mm:ssZ");
			var postParms = $"{{ \"values\": [ {{ \"timestamp\":\"{timestamp}\", \"value\": \"{jsonPayload.Replace("\"", "&quot;")}\" }} ] }}";
			await _m2xService.PostStreamValues("6e7bb7923219c6b72728cb1a34d0d5b6", "locations", postParms);
		}

		public async void PostLicensePlate()
		{
			var jsonPayload = JsonConvert.SerializeObject(new Models.FoundPlate()
			{
				Location = new GeoLocation { Latitude = 39, Longitude = -82 },
				Plate = "E61 5WS",
				TimeStamp = DateTime.Now.ToJSONString()
			});

			var timestamp = DateTime.UtcNow.ToJSONString();//.ToString("yyyy-MM-ddTHH:mm:ssZ");
			var postParms = $"{{ \"values\": [ {{ \"timestamp\":\"{timestamp}\", \"value\": \"{jsonPayload.Replace("\"", "&quot;")}\" }} ] }}";
			await _m2xService.PostStreamValues("6e7bb7923219c6b72728cb1a34d0d5b6", "licenseplate", postParms);
		}

		public ICommand PostLocationCommand { get; private set; }

		public ICommand PostLicensePlateCommand { get; private set; }

		public ObservableCollection<string> RecognizedTags { get { return _recognizedTags; } }

		private async void InitializeM2XServices()
		{
			await _m2xService.SetupMqttClient(_client_MqttMsgPublishReceived);
		}

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
