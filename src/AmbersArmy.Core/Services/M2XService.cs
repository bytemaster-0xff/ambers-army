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

		public async Task SetupMqttClient()
		{
			var hostNameIpAddress = string.Empty;
			var hn = new Windows.Networking.HostName(ApiEndpointHostName);
			using (var socket = new Windows.Networking.Sockets.StreamSocket())
			{
				await socket.ConnectAsync(hn, "http");
				hostNameIpAddress = socket.Information.RemoteAddress.DisplayName;
			}
			Client = new MqttClient(hostNameIpAddress);
			Client.MqttMsgPublishReceived += _client_MqttMsgPublishReceived;
			Client.Connect(ClientId, M2XMasterApiKey, null);
			if (Client.IsConnected)
			{
				/*
					* Regarding qosLevel... (from http://www.hivemq.com/blog/mqtt-essentials-part-6-mqtt-quality-of-service-levels)
					*
					* Use QoS 0 when …
					* - You have a complete or almost stable connection between sender and receiver. A classic use case is when connecting a test client or a front end application to a MQTT broker over a wired connection.
					* - You don’t care if one or more messages are lost once a while. That is sometimes the case if the data is not that important or will be send at short intervals, where it is okay that messages might get lost.
					* - You don’t need any message queuing. Messages are only queued for disconnected clients if they have QoS 1 or 2 and a persistent session.
					* 
					* Use QoS 1 when …
					* - You need to get every message and your use case can handle duplicates. The most often used QoS is level 1, because it guarantees the message arrives at least once. Of course your application must be tolerating duplicates and process them accordingly.
					* - You can’t bear the overhead of QoS 2. Of course QoS 1 is a lot fast in delivering messages without the guarantee of level 2.
					* 
					* Use QoS 2 when …
					* - It is critical to your application to receive all messages exactly once. This is often the case if a duplicate delivery would do harm to application users or subscribing clients. You should be aware of the overhead and that it takes a bit longer to complete the QoS 2 flow.
					* 
					* Queuing of QoS 1 and 2 messages
					* - All messages sent with QoS 1 and 2 will also be queued for offline clients, until they are available again. But queuing is only happening, if the client has a persistent session.
				*/
				Client.Subscribe(new[] { ApiResponseTopic, ApiCommandsTopic }, new[] { (byte)1, (byte)1 });
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