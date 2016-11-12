using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATTM2X;
using Newtonsoft.Json;

namespace AmbersArmy.Core.Services
{
	public class M2XService
	{
		private string M2XMasterApiKey { get; set; }

		public M2XService(string m2xMasterApiKey = null)
		{
			M2XMasterApiKey = !string.IsNullOrWhiteSpace(m2xMasterApiKey)
				? m2xMasterApiKey
				: Constants.M2XMasterApiKey;
		}

		public async Task<string> GetDeviceStream(string deviceId, string streamName)
		{
			var m2xClient = new M2XClient(M2XMasterApiKey);
			var device = m2xClient.Device(deviceId);
			var stream = device.Stream(streamName);
			var requestResult = await stream.Values();

			return !string.IsNullOrWhiteSpace(requestResult.Raw)
				? requestResult.Raw
				: string.Empty;
		}

		public async Task<T> GetDeviceStream<T>(string deviceId, string streamName) where T : class
		{
			var json = await GetDeviceStream(deviceId, streamName);
			T result = !string.IsNullOrWhiteSpace(json)
				? JsonConvert.DeserializeObject<T>(json)
				: null;

			return result;
		}
	}
}
