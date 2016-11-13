using ATTM2X;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AmbersArmy.M2XProxies
{
	public class Operations
	{
		public async Task<IEnumerable<Models.FoundPlate>> GetUpdatedStreamData(string masterApiKey, string deviceId, string streamName)
		{
			var client = new M2XClient(masterApiKey);
			var device = client.Device(deviceId);
			var stream = device.Stream(streamName);
			var data = await stream.Values();
			//var json = data.Raw.Replace("&quot;", "\"");
			////var result = JsonConvert.DeserializeObject<M2XLicensePlateStreamResponseModel>(json);
			var output = new List<Models.FoundPlate>();
			var rand = new Random(((int)DateTime.Now.Ticks / 100000));
			for (var i = 0; i < rand.Next(1, 10); i++)
			{
				output.Add(new Models.FoundPlate
				{
					DeviceId = "Acive Device",
					Location = new Models.GeoLocation { Latitude = 34.3, Longitude = 33.3 },
					Plate = "EA375A",
					TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.ffffZ")
				});
			}
			return output;
		}
	}

	public class M2XLicensePlateStreamResponseModel
	{
		public int limit { get; set; }
		public string end { get; set; }
		public object values { get; set; }
	}

	public class FoundPlateCaptured
	{
		public string timestamp { get; set; }
		public object value { get; set; }
	}


}