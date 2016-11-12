using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATTM2X;
using uPLibrary.Networking.M2Mqtt;

using Newtonsoft.Json;
using System.Reflection;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace AmbersArmy.Core.Services
{
	public class M2XService
	{
		private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
		private const string ApiEndpointHostName = "api-m2x.att.com";
		private const string ApiVersion = "v2";
		private static string ApiEndPoint = string.Format("mqtt://{0}@{1}", "{0}", ApiEndpointHostName);
		private static string ApiEndPointSecure = string.Format("mqtts://{0}@{1}", "{0}", ApiEndpointHostName);

		private string UserAgent;
		
		private string DeviceId { get; set; }
		private string EndPoint { get; set; }

		private string M2XMasterApiKey { get; set; }
		private string FlowApiKey { get; set; }

		private string ApiRequestTopic
		{
			get { return $"m2x/{M2XMasterApiKey}/requests"; }
		}

		private string ApiCommandsTopic
		{
			get { return $"m2x/{M2XMasterApiKey}/commands"; }
		}

		private string ApiResponseTopic
		{
			get { return $"m2x/{M2XMasterApiKey}/responses"; }
		}

		private static string ClientId = Guid.NewGuid().ToString();

		private MqttClient Client { get; set; }

		public Dictionary<string, List<object>> ApiResponses { get; set; }

		private int _numberOfApiResponsesToKeep = 1000;
		public int NumberOfApiResponsesToKeep
		{
			get { return _numberOfApiResponsesToKeep; }
			set { _numberOfApiResponsesToKeep = value; }
		}

		public M2XService(string m2xMasterApiKey = null, string flowApiKey = null)
		{
			M2XMasterApiKey = !string.IsNullOrWhiteSpace(m2xMasterApiKey)
				? m2xMasterApiKey
				: Constants.M2XMasterApiKey;

			FlowApiKey = !string.IsNullOrWhiteSpace(flowApiKey)
				? flowApiKey
				: null;

			var targetType = typeof(M2XClient).GetTypeInfo().Assembly.GetName();
			var version = targetType.Version.ToString();
			var langVersion = "4.5"; //Environment.Version.ToString();
			var osVersion = "unknown"; //Environment.OSVersion.ToString();
			UserAgent = string.Format("M2X-MQTT-.NET/{0} .NETFramework/{1} ({2})", version, langVersion, osVersion);
			if (ApiResponses == null) { ApiResponses = new Dictionary<string, List<object>>(); }
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



		private void _client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
		{
			// keep the responses list from occupying too much memory unnecessarily.
			if (ApiResponses.Count >= NumberOfApiResponsesToKeep) { ApiResponses.Clear(); }

			if (!ApiResponses.ContainsKey(e.Topic))
			{
				ApiResponses.Add(e.Topic, new List<object> { Encoding.UTF8.GetString(e.Message, 0, e.Message.Length) });
			}
			else
			{
				ApiResponses[e.Topic].Add(Encoding.UTF8.GetString(e.Message, 0, e.Message.Length));
			}
		}

		public void Dispose()
		{
			if (Client != null && Client.IsConnected)
			{
				Client.Disconnect();
				Client = null;
			}
		}

		~M2XService()
		{
			if (Client != null)
			{
				if (Client.IsConnected)
				{
					Client.Disconnect();
				}
				Client = null;
			}
		}
	}
}
