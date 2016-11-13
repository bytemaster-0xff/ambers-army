using System.Collections.Generic;
using System.Threading.Tasks;
using ATTM2X;
using AmbersArmy.Core.Interfaces;

namespace AmbersArmy.UWP.Common.Services
{
	public class M2XMqttService : IM2XMqttService
	{
		private string M2XMasterApiKey = "a548ac394631771d7ec6b86da4ca35e2";
		private string M2XKnownDeviceId = "6e7bb7923219c6b72728cb1a34d0d5b6";

		public M2XMqttService(string m2xMasterApiKey = null)
		{
			if (!string.IsNullOrWhiteSpace(m2xMasterApiKey))
			{
				M2XMasterApiKey = m2xMasterApiKey;
			}
		}

		public async Task PostStreamValues(string deviceId, string streamName, string values)
		{
			if (string.IsNullOrWhiteSpace(deviceId))
			{
				deviceId = M2XKnownDeviceId;
			}

			var client = new M2XClient(M2XMasterApiKey, deviceId);
			var device = client.Device(deviceId);
			var stream = device.Stream(streamName);
			var result = await stream.PostValues(values);
		}

		public async Task PostStreamValues<T>(string deviceId, string streamName, List<T> values)
		{
			if (string.IsNullOrWhiteSpace(deviceId))
			{
				deviceId = M2XKnownDeviceId;
			}

			var client = new M2XClient(M2XMasterApiKey, deviceId);
			var device = client.Device(deviceId);
			var stream = device.Stream(streamName);
			var result = await stream.PostValues(values);
		}
	}
}