using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Text;

namespace ATTM2X.Tests
{
	[TestClass]
	public class DeviceApiSpecs
	{
		private const string _masterApiKey = "your_master_api_key_goes_here";
		private static string TestDeviceId { get; set; }
		private static string TestDeviceSerial { get; set; }

		[ClassCleanup]
		public static async Task CleanupClass()
		{
			using (var client = new M2XClient(_masterApiKey))
			{
				var deviceWrapper = client.Device(TestDeviceId);
				var result = await deviceWrapper.Delete();
				Assert.IsTrue(result.IsPublished);
				await Task.Delay(1000);
			}
		}

		[TestInitialize]
		public async Task InitializeTest()
		{
			if (string.IsNullOrWhiteSpace(TestDeviceId))
			{
				TestDeviceSerial = $"test-{DateTime.UtcNow.Ticks}";
				using (var client = new M2XClient(_masterApiKey))
				{
					var deviceParameters = $"{{ \"base_device\": \"d781ab7460136af9db496c97172a6e6c\", \"name\": \"*** PLEASE DELETE ME *** Test Auto Created Device {DateTime.UtcNow.Ticks}\", \"visibility\": \"private\", \"description\": \"This is just a test device\", \"tags\": \"tag 1, tag 2, tag3\", \"serial\": \"{TestDeviceSerial}\", \"metadata\": {{ \"owner\": \"SkyNet\", \"favorite_player\": \"Stephen C.\" }} }}";
					var result = await client.CreateDevice(deviceParameters);

					Assert.IsNotNull(result);
					Assert.IsTrue(result.IsPublished);
					var apiResponse = await AssertResponseMessageWasReceived(client, result);

					var response = JsonConvert.DeserializeObject<ApiMqttTopicResponse>(apiResponse);
					var device = JsonConvert.DeserializeObject<Device>(response.body.ToString());
					Assert.IsNotNull(device);
					TestDeviceId = device.id;
					TestDeviceSerial = device.serial;
				}
			}
		}

		private async Task<string> AssertResponseMessageWasReceived(M2XClient client, M2XResponse response, bool shouldBeSuccessful = true)
		{
			var result = string.Empty;
			var cannotExit = true;
			while (cannotExit)
			{
				if (client.ApiResponses.Any(a => a.Value.Any(aa => (aa as string) != null && ((string)aa).Contains(response.RequestId))))
				{
					var topic = client.ApiResponses.First(f => f.Value.Any(aa => (aa as string) != null && ((string)aa).Contains(response.RequestId)));
					result = topic.Value.Where(w => (w as string) != null && ((string)w).Contains(response.RequestId)).First().ToString();
					var resultLowered = result.ToLowerInvariant();
					if (shouldBeSuccessful)
					{
						Assert.IsTrue(
							string.IsNullOrWhiteSpace(result) || resultLowered.Contains("accepted") || resultLowered.Contains(":200,") || resultLowered.Contains(": 200,")
							|| (!resultLowered.Contains("error") && !result.Contains(":403,") && !result.Contains(": 403,") && !result.Contains(":404,") && !result.Contains(": 404,") && !resultLowered.Contains("forbidden"))
							, $"{Environment.NewLine}Response:{Environment.NewLine}------------{Environment.NewLine}{result}");
					}
					else
					{
						Assert.IsTrue(resultLowered.Contains("error") || result.Contains("403") || resultLowered.Contains("forbidden"), $"Response does not indicate failure.");
					}
					cannotExit = false;
				}
				if (cannotExit) { await Task.Delay(250); }
			}
			Assert.IsFalse(cannotExit);
			return result;
		}

		[TestMethod]
		public async Task CanCreate_AndDelete_Private_Device_WithAll_RequiredParameters()
		{
			using (var client = new M2XClient(_masterApiKey))
			{
				var deviceParameters = $"{{ \"name\": \"*** PLEASE DELETE ME *** Test Auto Created Device {DateTime.UtcNow.Ticks}\", \"visibility\": \"private\" }}";
				var result = await client.CreateDevice(deviceParameters);

				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				var apiResponse = await AssertResponseMessageWasReceived(client, result);

				var response = JsonConvert.DeserializeObject<ApiMqttTopicResponse>(apiResponse);
				var device = JsonConvert.DeserializeObject<Device>(response.body.ToString());
				Assert.IsNotNull(device);

				var deviceWrapper = client.Device(device.id);
				Assert.IsNotNull(deviceWrapper);
				var deleteResult = await deviceWrapper.Delete();
				Assert.IsTrue(deleteResult.IsPublished);
				await AssertResponseMessageWasReceived(client, deleteResult);

				var devicesCheck = await client.SearchDevices($"{{ \"ids\": \"{device.id}\" }}");
				Assert.IsNotNull(devicesCheck);
				Assert.IsTrue(devicesCheck.IsPublished);
				await AssertResponseMessageWasReceived(client, devicesCheck);
			}
		}

		[TestMethod]
		public async Task CanCreate_AndDelete_Public_Device_WithAll_RequiredParameters()
		{
			using (var client = new M2XClient(_masterApiKey))
			{
				var deviceParameters = $"{{ \"name\": \"*** PLEASE DELETE ME *** Test Auto Created Device {DateTime.UtcNow.Ticks}\", \"visibility\": \"public\" }}";
				var result = await client.CreateDevice(deviceParameters);

				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				var apiResponse = await AssertResponseMessageWasReceived(client, result);

				var response = JsonConvert.DeserializeObject<ApiMqttTopicResponse>(apiResponse);
				var device = JsonConvert.DeserializeObject<Device>(response.body.ToString());
				Assert.IsNotNull(device);

				var deviceWrapper = client.Device(device.id);
				Assert.IsNotNull(deviceWrapper);
				var deleteResult = await deviceWrapper.Delete();
				Assert.IsTrue(deleteResult.IsPublished);
				await AssertResponseMessageWasReceived(client, deleteResult);

				var devicesCheck = await client.SearchDevices($"{{ \"ids\": \"{device.id}\" }}");
				Assert.IsNotNull(devicesCheck);
				Assert.IsTrue(devicesCheck.IsPublished);
				await AssertResponseMessageWasReceived(client, devicesCheck);
			}
		}

		[TestMethod]
		public async Task CanCreate_AndDelete_Private_Device_WithAll_AllowedLocalParameters()
		{
			using (var client = new M2XClient(_masterApiKey))
			{
				var deviceParameters = $"{{ \"name\": \"*** PLEASE DELETE ME *** Test Auto Created Device {DateTime.UtcNow.Ticks}\", \"visibility\": \"private\", \"description\": \"This is just a test device\", \"tags\": \"tag 1, tag 2, tag3\", \"serial\": \"test-{DateTime.UtcNow.Ticks}\", \"metadata\": {{ \"owner\": \"SkyNet\", \"favorite_player\": \"Stephen C.\" }} }}";
				var result = await client.CreateDevice(deviceParameters);

				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				var apiResponse = await AssertResponseMessageWasReceived(client, result);

				var response = JsonConvert.DeserializeObject<ApiMqttTopicResponse>(apiResponse);
				var device = JsonConvert.DeserializeObject<Device>(response.body.ToString());
				Assert.IsNotNull(device);

				var deviceWrapper = client.Device(device.id);
				Assert.IsNotNull(deviceWrapper);
				var deleteResult = await deviceWrapper.Delete();
				Assert.IsTrue(deleteResult.IsPublished);
				await AssertResponseMessageWasReceived(client, deleteResult);

				var devicesCheck = await client.SearchDevices($"{{ \"ids\": \"{device.id}\" }}");
				Assert.IsNotNull(devicesCheck);
				Assert.IsTrue(devicesCheck.IsPublished);
				await AssertResponseMessageWasReceived(client, devicesCheck);
			}
		}

		[TestMethod]
		public async Task CanDuplicate_AndDelete_Device_WithNo_AdditionalParameters()
		{
			using (var client = new M2XClient(_masterApiKey))
			{
				var deviceParameters = $"{{ \"base_device\": \"cd85543b1ba7299db205470ebb935117\" }}";
				var result = await client.CreateDevice(deviceParameters);

				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				var apiResponse = await AssertResponseMessageWasReceived(client, result);

				var response = JsonConvert.DeserializeObject<ApiMqttTopicResponse>(apiResponse);
				var device = JsonConvert.DeserializeObject<Device>(response.body.ToString());
				Assert.IsNotNull(device);

				var deviceWrapper = client.Device(device.id);
				Assert.IsNotNull(deviceWrapper);
				var deleteResult = await deviceWrapper.Delete();
				Assert.IsTrue(deleteResult.IsPublished);
				await AssertResponseMessageWasReceived(client, deleteResult);

				var devicesCheck = await client.SearchDevices($"{{ \"ids\": \"{device.id}\" }}");
				Assert.IsNotNull(devicesCheck);
				Assert.IsTrue(devicesCheck.IsPublished);
				await AssertResponseMessageWasReceived(client, devicesCheck);
			}
		}

		[TestMethod]
		public async Task CanDuplicate_AndDelete_Device_With_SubstitutionParameters()
		{
			using (var client = new M2XClient(_masterApiKey))
			{
				var deviceParameters = $"{{ \"base_device\": \"cd85543b1ba7299db205470ebb935117\", \"name\": \"*** PLEASE DELETE ME *** Test Auto Created Device {DateTime.UtcNow.Ticks}\", \"visibility\": \"private\", \"description\": \"This is just a test device\", \"tags\": \"tag 1, tag 2, tag3\", \"serial\": \"test-{DateTime.UtcNow.Ticks}\", \"metadata\": {{ \"owner\": \"SkyNet\", \"favorite_player\": \"Stephen C.\" }} }}";
				var result = await client.CreateDevice(deviceParameters);

				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				var apiResponse = await AssertResponseMessageWasReceived(client, result);

				var response = JsonConvert.DeserializeObject<ApiMqttTopicResponse>(apiResponse);
				var device = JsonConvert.DeserializeObject<Device>(response.body.ToString());
				Assert.IsNotNull(device);

				var deviceWrapper = client.Device(device.id);
				Assert.IsNotNull(deviceWrapper);
				var deleteResult = await deviceWrapper.Delete();
				Assert.IsTrue(deleteResult.IsPublished);
				await AssertResponseMessageWasReceived(client, deleteResult);

				var devicesCheck = await client.SearchDevices($"{{ \"ids\": \"{device.id}\" }}");
				Assert.IsNotNull(devicesCheck);
				Assert.IsTrue(devicesCheck.IsPublished);
				await AssertResponseMessageWasReceived(client, devicesCheck);
			}
		}

		[TestMethod]
		public async Task CanDuplicate_AndUpdate_AndDelete_Device_With_SubstitutionParameters()
		{
			using (var client = new M2XClient(_masterApiKey))
			{
				var deviceParameters = $"{{ \"base_device\": \"cd85543b1ba7299db205470ebb935117\", \"name\": \"*** PLEASE DELETE ME *** Test Auto Created Device {DateTime.UtcNow.Ticks}\" }}";
				var result = await client.CreateDevice(deviceParameters);

				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				var apiResponse = await AssertResponseMessageWasReceived(client, result);

				var response = JsonConvert.DeserializeObject<ApiMqttTopicResponse>(apiResponse);
				var device = JsonConvert.DeserializeObject<Device>(response.body.ToString());
				Assert.IsNotNull(device);

				var deviceWrapper = client.Device(device.id);
				Assert.IsNotNull(deviceWrapper);

				var updateParameters = $"{{ \"name\": \"*** PLEASE DELETE ME *** This Device has been updated at {DateTime.Now}\", \"visibility\": \"private\", \"description\": \"Blah blah blah test device\", \"tags\": \"tag 4, tag 5, tag6\", \"serial\": \"test-{DateTime.UtcNow.Ticks}\", \"metadata\": {{ \"owner\": \"The Justice League\", \"favorite_player\": \"Labron J.\" }} }}";
				var updateResult = await deviceWrapper.Update(updateParameters);
				Assert.IsTrue(updateResult.IsPublished);
				await AssertResponseMessageWasReceived(client, updateResult);

				var deleteResult = await deviceWrapper.Delete();
				Assert.IsTrue(deleteResult.IsPublished);
				await AssertResponseMessageWasReceived(client, deleteResult);

				var devicesCheck = await client.SearchDevices($"{{ \"ids\": \"{device.id}\" }}");
				Assert.IsNotNull(devicesCheck);
				Assert.IsTrue(devicesCheck.IsPublished);
				await AssertResponseMessageWasReceived(client, devicesCheck);
			}
		}

		[TestMethod]
		public async Task CanAccess_ApiKey_SingleDevice_ById_AndView_DeviceDetails()
		{
			using (var client = new M2XClient(_masterApiKey))
			{
				var device = client.Device(TestDeviceId);
				var result = await device.Details();
				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				await AssertResponseMessageWasReceived(client, result);
			}
		}

		[TestMethod]
		public async Task CanAccess_ApiKey_SingleDevice_ById_AndUpdate_Location()
		{
			using (var client = new M2XClient(_masterApiKey))
			{
				var device = client.Device(TestDeviceId);
				var updateParms = "{ \"name\": \"Updated!!!\", \"latitude\": 28.375252, \"longitude\": -81.549370, \"elevation\": 100.52, \"timestamp\": \"2016-05-20T15:03:32.006Z\" }";
				var result = await device.UpdateLocation(updateParms);
				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				await AssertResponseMessageWasReceived(client, result);
			}
		}

		[TestMethod]
		public async Task CanAccess_ApiKey_SingleDevice_ById_AndUpdate_ExistingStream()
		{
			using (var client = new M2XClient(_masterApiKey))
			{
				var device = client.Device(TestDeviceId);
				var stream = device.Stream(Constants.TestStreamName002);
				var updateParms = "{ \"display_name\": \"Not for polite company!\" }";
				var result = await stream.Update(updateParms);
				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				await AssertResponseMessageWasReceived(client, result);

				var resetParms = "{ \"display_name\": \"Movements\" }";
				var resetResult = await stream.Update(resetParms);
				Assert.IsNotNull(resetResult);
				Assert.IsTrue(resetResult.IsPublished);
				await AssertResponseMessageWasReceived(client, resetResult);
			}
		}

		[TestMethod]
		public async Task CanAccess_ApiKey_SingleDevice_ById_AndCreate_NewSteam_ThenAddValues_ThenDelete_SomeStreamValues_ThenDelete_ThatStream()
		{
			var testStreamName = "testnumeric999";
			DateTime tsBasis = DateTime.UtcNow.AddMinutes(-10);
			using (var client = new M2XClient(_masterApiKey))
			{
				var device = client.Device(TestDeviceId);
				var stream = device.Stream(testStreamName);
				var createParms = "{ \"display_name\": \"To be deleted!\", \"type\": \"numeric\" }";
				var result = await stream.Update(createParms);
				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				await AssertResponseMessageWasReceived(client, result);

				var values = new StringBuilder();
				for (var i = 0; i < 10; i++)
				{
					var ts = tsBasis.AddMinutes(i);
					if (values.Length > 0) { values.Append(", "); }
					values.Append($"{{ \"timestamp\": \"{ts.ToString("yyyy-MM-ddTHH:mm:00Z")}\", \"value\": {i} }}");
				}

				var valueParms00 = $"{{ \"values\": [ {values.ToString()} ] }}";
				var resultValue00 = await stream.PostValues(valueParms00);
				Assert.IsNotNull(resultValue00);
				Assert.IsTrue(resultValue00.IsPublished);
				await AssertResponseMessageWasReceived(client, resultValue00);

				var checkDetails = await stream.Details();
				Assert.IsTrue(checkDetails.IsPublished);
				await AssertResponseMessageWasReceived(client, checkDetails);

				var deleteFrom = tsBasis;
				var deleteParams = $"{{ \"from\": \"{deleteFrom.ToString("yyyy-MM-ddTHH:mm:ss.000Z")}\", \"end\": \"{ deleteFrom.AddMinutes(5).ToString("yyyy-MM-ddTHH:mm:ss.999Z")}\" }}";

				var deleteValuesResult = await stream.DeleteValues(deleteParams);
				Assert.IsTrue(deleteValuesResult.IsPublished);
				await AssertResponseMessageWasReceived(client, deleteValuesResult);

				var checkDetailsAgainResult = await stream.Details();
				Assert.IsTrue(checkDetailsAgainResult.IsPublished);
				await AssertResponseMessageWasReceived(client, checkDetailsAgainResult);

				var reset = await stream.Delete();
				Assert.IsNotNull(reset);
				Assert.IsTrue(reset.IsPublished);
				await AssertResponseMessageWasReceived(client, reset);
			}
		}

		[TestMethod]
		public async Task CanAccess_ApiKey_SingleDevice_ById_AndPost_SingleValues_ToMultiple_Streams()
		{
			DateTime tsBasis = DateTime.UtcNow.AddMinutes(-10);
			var testStreamName00 = "testnumeric900";
			var testStreamName01 = "testnumeric901";
			var streams = new[] { testStreamName00, testStreamName01 };

			using (var client = new M2XClient(_masterApiKey))
			{
				var device = client.Device(TestDeviceId);
				foreach (var testStreamName in streams)
				{
					var stream = device.Stream(testStreamName);
					var createParms = $"{{ \"display_name\": \"{testStreamName}!\", \"type\": \"numeric\" }}";
					var result = await stream.Update(createParms);
					Assert.IsTrue(result.IsPublished);
					await AssertResponseMessageWasReceived(client, result);
				}

				var postParam = $"{{ \"timestamp\": \"{tsBasis.ToString("yyyy-MM-ddTHH:mm:00Z")}\", \"values\": {{ \"{testStreamName00}\": 44, \"{testStreamName01}\": 55 }} }}";

				var resultValue00 = await device.PostUpdate(postParam);
				Assert.IsNotNull(resultValue00);
				Assert.IsTrue(resultValue00.IsPublished);
				await AssertResponseMessageWasReceived(client, resultValue00);

				foreach (var testStreamName in streams)
				{
					var stream = device.Stream(testStreamName);
					var reset = await stream.Delete();
					Assert.IsNotNull(reset);
					Assert.IsTrue(reset.IsPublished);
					await AssertResponseMessageWasReceived(client, reset);
				}
			}
		}

		[TestMethod]
		public async Task CanAccess_ApiKey_SingleDevice_ById_AndPost_MultipleValues_ToMultiple_Streams()
		{
			DateTime tsBasis = DateTime.UtcNow.AddMinutes(-10);
			var testStreamName00 = "testnumeric900";
			var testStreamName01 = "testnumeric901";
			var streams = new[] { testStreamName00, testStreamName01 };
			var aggregateStreamsPayload = new StringBuilder();
			var payloadFormat = "{{ \"values\": {{ {0} }} }}";
			var streamPayloadFormat = "\"{0}\": [ {1} ]";
			var valuesPayloadFormat = "{{ \"timestamp\": \"{0}\", \"value\": {1} }}";

			using (var client = new M2XClient(_masterApiKey))
			{
				var device = client.Device(TestDeviceId);
				foreach (var testStreamName in streams)
				{
					var stream = device.Stream(testStreamName);
					var createParms = $"{{ \"display_name\": \"{testStreamName}!\", \"type\": \"numeric\" }}";
					var result = await stream.Update(createParms);
					Assert.IsNotNull(result);
					Assert.IsTrue(result.IsPublished);
					await AssertResponseMessageWasReceived(client, result);

					var rand = new Random((int)DateTime.UtcNow.Ticks);
					var tempValues = new StringBuilder();
					for (var i = 0; i < rand.Next(5, 20); i++)
					{
						if (tempValues.Length > 0) { tempValues.Append(", "); }
						tempValues.AppendFormat(valuesPayloadFormat, tsBasis.AddSeconds(rand.Next(0, 1000)).ToString("yyyy-MM-ddTHH:mm:00Z"), rand.Next(0, 1000));
					}
					if (aggregateStreamsPayload.Length > 0) { aggregateStreamsPayload.Append(", "); }
					aggregateStreamsPayload.AppendFormat(streamPayloadFormat, testStreamName, tempValues);
				}

				var postParam = string.Format(payloadFormat, aggregateStreamsPayload);

				var resultValue00 = await device.PostUpdates(postParam);
				Assert.IsNotNull(resultValue00);
				Assert.IsTrue(resultValue00.IsPublished);
				await AssertResponseMessageWasReceived(client, resultValue00);

				foreach (var testStreamName in streams)
				{
					var stream = device.Stream(testStreamName);
					var reset = await stream.Delete();
					Assert.IsNotNull(reset);
					Assert.IsTrue(reset.IsPublished);
					await AssertResponseMessageWasReceived(client, reset);
				}
			}
		}

	}

	public class ApiMqttTopicResponse
	{
		public object id { get; set; }
		public int status { get; set; }
		public string error { get; set; }
		public object body { get; set; }
	}

	public class Device
	{
		public string url { get; set; }
		public string name { get; set; }
		public string status { get; set; }
		public string serial { get; set; }
		public object[] tags { get; set; }
		public string visibility { get; set; }
		public object description { get; set; }
		public DateTime created { get; set; }
		public DateTime updated { get; set; }
		public DateTime last_activity { get; set; }
		public object location { get; set; }
		public string id { get; set; }
		public Streams streams { get; set; }
		public object metadata { get; set; }
		public string key { get; set; }
		public Triggers triggers { get; set; }
	}

	public class Streams
	{
		public int count { get; set; }
		public string url { get; set; }
	}

	public class Triggers
	{
		public int count { get; set; }
		public string url { get; set; }
	}
}
