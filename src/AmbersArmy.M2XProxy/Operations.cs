using ATTM2X;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AmbersArmy.M2XProxy
{
	public class Operations
	{
		public async Task<IEnumerable<Models.FoundPlate>> GetUpdatedStreamData(string masterApiKey, string deviceId, string streamName)
		{
			var client = new M2XClient(masterApiKey);
			var device = client.Device(deviceId);
			var stream = device.Stream(streamName);
			var data = await stream.Values();
			var json = data.Raw;
			var result = JsonConvert.DeserializeObject<List<Models.FoundPlate>>(json);
			return result;
		}
	}
}