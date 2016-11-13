using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ATTM2X
{
	/// <summary>
	/// Wrapper for AT&T M2X API
	/// https://m2x.att.com/developer/documentation/v2/overview
	/// </summary>
	public sealed class M2XClient : IDisposable
	{
		private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
		private const string ApiEndpointHostName = "api-m2x.att.com";
		private const string ApiVersion = "v2";
		private static string ApiEndPoint = string.Format("mqtt://{0}@{1}", "{0}", ApiEndpointHostName);
		private static string ApiEndPointSecure = string.Format("mqtts://{0}@{1}", "{0}", ApiEndpointHostName);

		private static readonly string UserAgent;

		private static string APIKey { get; set; }
		private static string DeviceId { get; set; }
		private static string EndPoint { get; set; }

		private static string ApiRequestTopic
		{
			get { return $"m2x/{APIKey}/requests"; }
		}

		private static string ApiCommandsTopic
		{
			get { return $"m2x/{APIKey}/commands"; }
		}

		private static string ApiResponseTopic
		{
			get { return $"m2x/{APIKey}/responses"; }
		}

		private static string ClientId = Guid.NewGuid().ToString();

		private MqttClient Client { get; set; }

		public Dictionary<string, List<object>> ApiResponses { get; set; }

		private int _numberOfApiResponsesToKeep = 5;
		public int NumberOfApiResponsesToKeep
		{
			get { return _numberOfApiResponsesToKeep; }
			set { _numberOfApiResponsesToKeep = value; }
		}

		static M2XClient()
		{
			var targetType = typeof(M2XClient).GetTypeInfo().Assembly.GetName();
			var version = targetType.Version.ToString();
			var langVersion = "4.5"; //Environment.Version.ToString();
			var osVersion = "unknown"; //Environment.OSVersion.ToString();
			UserAgent = string.Format("M2X-MQTT-.NET/{0} .NETFramework/{1} ({2})", version, langVersion, osVersion);
		}

		public M2XClient(string apiKey, string deviceId = null, string m2xApiEndPoint = null, MqttClient mqttClient = null)
		{
			if (string.IsNullOrWhiteSpace(m2xApiEndPoint))
			{
				m2xApiEndPoint = ApiEndPoint;
			}

			APIKey = apiKey;
			if (!string.IsNullOrWhiteSpace(deviceId)) { DeviceId = deviceId; }
			EndPoint = m2xApiEndPoint.Contains("{")
				? string.Format(m2xApiEndPoint, APIKey)
				: m2xApiEndPoint;

			if (ApiResponses == null) { ApiResponses = new Dictionary<string, List<object>>(); }
			if (mqttClient != null) { Client = mqttClient; }
		}

		private async Task SetupMqttClient()
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
			Client.Connect(ClientId, APIKey, null);
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

		private void _client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
		{
			// keep the responses list from occupying too much memory unnecessarily.
			if (ApiResponses.Count >= 5) { ApiResponses.Clear(); }

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

		~M2XClient()
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

		// Device API

		/// <summary>
		/// Retrieve the list of devices accessible by the authenticated API key that meet the search criteria.
		///
		/// https://m2x.att.com/developer/documentation/v2/device#Search-Devices
		/// </summary>
		public Task<M2XResponse> SearchDevices(object parms = null)
		{
			return MakeRequest(new[] { M2XDevice.UrlPath, "search" }, M2XClientMethod.GET, parms);
		}

		/// <summary>
		/// Create a new device
		///
		/// https://m2x.att.com/developer/documentation/v2/device#Create-Device
		/// </summary>
		public Task<M2XResponse> CreateDevice(object parms)
		{
			return MakeRequest(new[] { M2XDevice.UrlPath }, M2XClientMethod.POST, parms);
		}

		/// <summary>
		/// Get a wrapper to access an existing Device.
		/// </summary>
		public M2XDevice Device(string deviceId)
		{
			return new M2XDevice(this, deviceId);
		}

		// Distribution API

		/// <summary>
		/// Get a wrapper to access an existing device distribution.
		/// </summary>
		public M2XDistribution Distribution(string distributionId)
		{
			return new M2XDistribution(this, distributionId);
		}

		// Time API

		/// <summary>
		/// Returns M2X servers' time.
		///
		/// https://m2x.att.com/developer/documentation/v2/time
		/// </summary>
		public Task<M2XResponse> Time(string format = null)
		{
			return MakeRequest(new[] { "time", !string.IsNullOrEmpty(format) ? format : null });
		}

		// Commands API

		/// <summary>
		/// Retrieve the list of recent commands sent by the current user (as given by the API key).
		///
		/// https://m2x.att.com/developer/documentation/v2/commands#List-Sent-Commands
		/// </summary>
		public Task<M2XResponse> Commands(object parms = null)
		{
			return MakeRequest(new[] { "commands" }, M2XClientMethod.GET, parms);
		}

		/// <summary>
		/// Send a command with the given name to the given target devices.
		///
		/// https://m2x.att.com/developer/documentation/v2/commands#Send-Command
		/// </summary>
		public Task<M2XResponse> SendCommand(object parms)
		{
			return MakeRequest(new[] { "commands" }, M2XClientMethod.POST, parms);
		}

		/// <summary>
		/// Get details of a sent command including the delivery information for all devices that were targetted by the command at the time it was sent.
		///
		/// https://m2x.att.com/developer/documentation/v2/commands#View-Command-Details
		/// </summary>
		public Task<M2XResponse> CommandDetails(string commandId)
		{
			return MakeRequest(new[] { "commands", commandId });
		}

		// Common

		/// <summary>
		/// Formats a DateTime value to an ISO8601 timestamp.
		/// </summary>
		private static string DateTimeToString(DateTime dateTime)
		{
			return dateTime.ToString(DateTimeFormat);
		}

		/// <summary>
		/// Builds url to AT&T M2X API with optional query parameters
		/// </summary>
		/// <param name="resourceIdentifier">AT&T M2X MQTT API resource identifiers, *NOT* the entire resource path! For example, new []{ "streams", "temp", "values" }</param>
		internal string BuildUrl(string[] resourceIdentifier)
		{
			var urlParts = new StringBuilder($"/{ApiVersion}");
			foreach (var p in resourceIdentifier.Where(w => !string.IsNullOrWhiteSpace(w)))
			{
				urlParts.Append("/");
				urlParts.Append(p.Replace("/", string.Empty));
			}
			return urlParts.ToString();
		}

		/// <summary>
		/// Builds url to AT&T M2X API with optional query parameters
		/// </summary>
		/// <param name="path">AT&T M2X API url path</param>
		/// <param name="parms">parameters to build url query</param>
		internal string BuildUrl(string path, object parms = null)
		{
			string fullUrl = EndPoint + path;
			if (parms != null)
			{
				string queryString = ObjectToQueryString(parms);
				if (!String.IsNullOrEmpty(queryString))
					fullUrl += "?" + queryString;
			}
			return fullUrl;
		}

		private static string ObjectToQueryString(object queryParams)
		{
			StringBuilder sb = new StringBuilder();
			IEnumerable<FieldInfo> fields = queryParams.GetType().GetFields();
			foreach (var prop in fields)
			{
				if (prop.IsStatic || !prop.IsPublic || prop.FieldType.IsArray)
					continue;
				object value = prop.GetValue(queryParams);
				AppendQuery(sb, prop.Name, value);
			}
			IEnumerable<PropertyInfo> props = queryParams.GetType().GetProperties();
			foreach (var prop in props)
			{
				if (!prop.CanRead || prop.PropertyType.IsArray)
					continue;
				object value = prop.GetValue(queryParams, null);
				AppendQuery(sb, prop.Name, value);
			}
			return sb.ToString();
		}

		private static void AppendQuery(StringBuilder sb, string name, object value)
		{
			if (value == null)
				return;
			if (sb.Length > 0)
				sb.Append('&');
			sb.Append(name).Append('=').Append(WebUtility.UrlEncode(value.ToString()));
		}


		/// <summary>
		/// Performs async M2X API request
		/// </summary>
		/// <param name="path">API path</param>
		/// <param name="parms">Query string (for GET and DELETE) or body (for POST and PUT) parameters</param>
		/// <param name="addBodyParms">Additional body parameters, if specified, the parms will be treated as query parameters.
		/// The passed object will be serialized into a JSON string. In case of a string passed it will be treated as a valid JSON string.</param>
		/// <returns>The request and response data from M2X server</returns>
		public async Task<M2XResponse> MakeRequest(string[] resourceIdentifier, M2XClientMethod method = M2XClientMethod.GET, object parms = null)
		{
			var result = CreateResponse(resourceIdentifier, method, parms);
			var messageContent = result.RequestContent;

			try
			{
				if (Client == null)
				{
					await SetupMqttClient();
				}

				if (!Client.IsConnected)
				{
					Client.Connect(ClientId, APIKey, null);
				}

				var publishMessage = Encoding.UTF8.GetBytes(messageContent);
				result.IsPublished = (Client.Publish(ApiRequestTopic, publishMessage)) > 0;
			}
			catch (Exception ex)
			{
				result.WebError = ex;
			}

			return result;
		}


		private M2XResponse CreateResponse(string[] resourceIdentifier, M2XClientMethod method, object parms)
		{
			var requestId = Guid.NewGuid().ToString().Replace("-", string.Empty);
			var resourceUrl = BuildUrl(resourceIdentifier);
			var content = $"{{ \"id\": \"{requestId}\", \"method\": \"{method.ToString()}\", \"resource\": \"{resourceUrl}\", \"agent\": \"{UserAgent}\", \"body\": {(parms != null ? parms : "null")} }}";

			Uri processedUri = !string.IsNullOrWhiteSpace(resourceUrl) ? new Uri(resourceUrl, UriKind.Relative) : null;
			var response = new M2XResponse(processedUri, content) { RequestId = requestId };
			return response;
		}
	}
}