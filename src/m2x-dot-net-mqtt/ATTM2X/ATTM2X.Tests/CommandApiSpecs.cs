using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Text;

namespace ATTM2X.Tests
{
	[TestClass]
	public class CommandApiSpecs
	{
		private const string _masterApiKey = "your_master_api_key_goes_here";
		private static string TestDeviceId { get; set; }
		private static string TestDeviceSerial { get; set; }

		#region " Initialize and Cleanup Methods "

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

		#endregion " Initialize and Cleanup Methods "

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
		public async Task CanAccess_MasterApiKey_ListOf_CommandsSent_NoFilters()
		{
			using (var client = new M2XClient(_masterApiKey))
			{
				var result = await client.Commands();
				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				await AssertResponseMessageWasReceived(client, result);
			}
		}

		[TestMethod]
		public async Task CanAccess_MasterApiKey_ListOf_CommandsSent_WithFilters()
		{
			using (var client = new M2XClient(_masterApiKey))
			{
				foreach (var filter in new[] { "limit", "page", "dir|desc", "dir|asc", "start", "end", "name" })
				{
					M2XResponse result = null;
					switch (filter)
					{
						case "limit":
							result = await client.Commands("{ \"limit\": 2 }");
							break;
						case "page":
							result = await client.Commands("{ \"page\": 1 }");
							break;
						case "dir|desc":
							result = await client.Commands("{ \"dir\": \"desc\" }");
							break;
						case "dir|asc":
							result = await client.Commands("{ \"dir\": \"asc\" }");
							break;
						case "start":
							result = await client.Commands($"{{ \"start\": \"{DateTime.UtcNow.AddMinutes(-60).ToString(Constants.ISO8601_DateStartFormat)}\" }}");
							break;
						case "end":
							result = await client.Commands($"{{ \"end\": \"{DateTime.UtcNow.AddMinutes(-10).ToString(Constants.ISO8601_DateStartFormat)}\" }}");
							break;
						case "name":
							result = await client.Commands("{ \"name\": \"PHONE_HOME\" }");
							break;
					}

					Assert.IsNotNull(result);
					Assert.IsTrue(result.IsPublished);
					await AssertResponseMessageWasReceived(client, result);
				}
			}
		}

		[TestMethod]
		public async Task CanAccess_MasterApiKey_AndSendCommands()
		{
			using (var client = new M2XClient(_masterApiKey))
			{
				var sendCommandParms = new StringBuilder($"{{ ");
				sendCommandParms.Append($"\"name\": \"PHONE_HOME\"");
				sendCommandParms.Append($", ");
				sendCommandParms.Append($"\"data\": {{ \"server_url\": \"https://m2x.att.com\" }}");
				sendCommandParms.Append($", ");
				sendCommandParms.Append($"\"targets\": {{ \"devices\": [\"{TestDeviceId}\"] }}");
				sendCommandParms.Append($" }}");

				var result = await client.SendCommand(sendCommandParms.ToString());
				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				await AssertResponseMessageWasReceived(client, result);
			}
		}

		[TestMethod]
		public async Task CanAccess_MasterApiKey_ListOf_CommandsSent_TakeOne_AndView_Details()
		{
			var targetCommandId = string.Empty;
			using (var client = new M2XClient(_masterApiKey))
			{
				var retrieveCommandsResult = await client.Commands();
				Assert.IsTrue(retrieveCommandsResult.IsPublished);
				var apiResponse = await AssertResponseMessageWasReceived(client, retrieveCommandsResult);
				var response = JsonConvert.DeserializeObject<ApiMqttTopicResponse>(apiResponse);
				var responseBody = JsonConvert.DeserializeObject<ApiMqttTopicResponseWithCommands>(response.body.ToString());

				var target = responseBody.commands.FirstOrDefault(fd => fd.status_counts.pending > 0);
				targetCommandId = target.id;

				var result = await client.CommandDetails(targetCommandId);
				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				await AssertResponseMessageWasReceived(client, result);
			}
		}

		[TestMethod]
		public async Task CanAccess_MasterApiKey_AndSendCommands_ToSingleDevice_AndList_DeviceCommands()
		{
			using (var client = new M2XClient(_masterApiKey))
			{
				var sendCommandParms = new StringBuilder($"{{ ");
				sendCommandParms.Append($"\"name\": \"PHONE_HOME\"");
				sendCommandParms.Append($", ");
				sendCommandParms.Append($"\"data\": {{ \"server_url\": \"https://m2x.att.com\" }}");
				sendCommandParms.Append($", ");
				sendCommandParms.Append($"\"targets\": {{ \"devices\": [\"{TestDeviceId}\"] }}");
				sendCommandParms.Append($" }}");

				var result = await client.SendCommand(sendCommandParms.ToString());
				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				await AssertResponseMessageWasReceived(client, result);
			}
		}

		[TestMethod]
		public async Task CanAccess_MasterApiKey_AndSendCommands_ToSingleDevice_AndDevice_MarksCommand_AsProcessed()
		{
			var targetCommandId = string.Empty;
			var commandName = "PHONE_HOME";
			using (var client = new M2XClient(_masterApiKey))
			{
				var sendCommandParms = new StringBuilder($"{{ ");
				sendCommandParms.Append($"\"name\": \"{commandName}\"");
				sendCommandParms.Append($", ");
				sendCommandParms.Append($"\"data\": {{ \"server_url\": \"https://m2x.att.com\" }}");
				sendCommandParms.Append($", ");
				sendCommandParms.Append($"\"targets\": {{ \"devices\": [\"{TestDeviceId}\"] }}");
				sendCommandParms.Append($" }}");

				var sendcommandResult = await client.SendCommand(sendCommandParms.ToString());
				Assert.IsNotNull(sendcommandResult);
				Assert.IsTrue(sendcommandResult.IsPublished);
				await AssertResponseMessageWasReceived(client, sendcommandResult);

				var retrieveCommandsResult = await client.Commands();
				Assert.IsTrue(retrieveCommandsResult.IsPublished);
				var apiResponse = await AssertResponseMessageWasReceived(client, retrieveCommandsResult);
				var response = JsonConvert.DeserializeObject<ApiMqttTopicResponse>(apiResponse);
				var responseBody = JsonConvert.DeserializeObject<ApiMqttTopicResponseWithCommands>(response.body.ToString());
				var oldCount = responseBody.commands.Where(w => w.status_counts.processed > 0).Count();

				var target = responseBody.commands.FirstOrDefault(fd => fd.status_counts.pending > 0);
				targetCommandId = target.id;

				var device = client.Device(TestDeviceId);
				var processParms = $"{{ \"madethecall\": \"today\" }}";
				var result = await device.ProcessCommand(targetCommandId, processParms);
				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				await AssertResponseMessageWasReceived(client, result);

				var checkForProcessedCommandsResult = await client.Commands();
				Assert.IsTrue(checkForProcessedCommandsResult.IsPublished);
				var checkApiResponse = await AssertResponseMessageWasReceived(client, checkForProcessedCommandsResult);
				var checkResponse = JsonConvert.DeserializeObject<ApiMqttTopicResponse>(checkApiResponse);
				var checkResponseBody = JsonConvert.DeserializeObject<ApiMqttTopicResponseWithCommands>(checkResponse.body.ToString());
				Assert.IsTrue(checkResponseBody.commands.Where(w => w.status_counts.processed > 0).Count() > oldCount);
			}
		}

		[TestMethod]
		public async Task CanAccess_MasterApiKey_AndSendCommands_ToSingleDevice_AndDevice_MarksCommand_AsRejected()
		{
			var targetCommandId = string.Empty;
			var commandName = "PHONE_HOME";
			using (var client = new M2XClient(_masterApiKey))
			{
				var sendCommandParms = new StringBuilder($"{{ ");
				sendCommandParms.Append($"\"name\": \"{commandName}\"");
				sendCommandParms.Append($", ");
				sendCommandParms.Append($"\"data\": {{ \"server_url\": \"https://m2x.att.com\" }}");
				sendCommandParms.Append($", ");
				sendCommandParms.Append($"\"targets\": {{ \"devices\": [\"{TestDeviceId}\"] }}");
				sendCommandParms.Append($" }}");

				var sendcommandResult = await client.SendCommand(sendCommandParms.ToString());
				Assert.IsNotNull(sendcommandResult);
				Assert.IsTrue(sendcommandResult.IsPublished);
				await AssertResponseMessageWasReceived(client, sendcommandResult);

				var device = client.Device(TestDeviceId);
				var retrieveCommandsResult = await device.Commands();
				Assert.IsTrue(retrieveCommandsResult.IsPublished);
				var apiResponse = await AssertResponseMessageWasReceived(client, retrieveCommandsResult);
				var response = JsonConvert.DeserializeObject<ApiMqttTopicResponse>(apiResponse);
				var responseBody = JsonConvert.DeserializeObject<ApiMqttTopicResponseWithCommands>(response.body.ToString());
				var oldCount = responseBody.commands.Where(w => w.status == "rejected").Count();

				var target = responseBody.commands.FirstOrDefault(fd => fd.status == "pending");
				targetCommandId = target.id;

				var rejectParms = $"{{ \"reason\": \"Because I can too!\" }}";
				var result = await device.RejectCommand(targetCommandId, rejectParms);
				Assert.IsNotNull(result);
				Assert.IsTrue(result.IsPublished);
				await AssertResponseMessageWasReceived(client, result);

				var checkForRejectedCommands = await device.Commands();
				Assert.IsTrue(checkForRejectedCommands.IsPublished);
				var checkApiResponse = await AssertResponseMessageWasReceived(client, checkForRejectedCommands);
				var checkResponse = JsonConvert.DeserializeObject<ApiMqttTopicResponse>(checkApiResponse);
				var checkResponseBody = JsonConvert.DeserializeObject<ApiMqttTopicResponseWithCommands>(checkResponse.body.ToString());
				Assert.IsTrue(checkResponseBody.commands.Where(w => w.status == "rejected").Count() > oldCount);
			}
		}

		[TestMethod]
		public async Task CanAccess_MasterApiKey_AndSendCommand_ToSingleDevice_ThenList_CommandsSent_WithFilters()
		{
			var targetCommandId = string.Empty;
			var commandName = "PHONE_HOME";
			using (var client = new M2XClient(_masterApiKey))
			{
				var sendCommandParms = new StringBuilder($"{{ ");
				sendCommandParms.Append($"\"name\": \"{commandName}\"");
				sendCommandParms.Append($", ");
				sendCommandParms.Append($"\"data\": {{ \"server_url\": \"https://m2x.att.com\" }}");
				sendCommandParms.Append($", ");
				sendCommandParms.Append($"\"targets\": {{ \"devices\": [\"{TestDeviceId}\"] }}");
				sendCommandParms.Append($" }}");

				var sendcommandResult = await client.SendCommand(sendCommandParms.ToString());
				Assert.IsNotNull(sendcommandResult);
				Assert.IsTrue(sendcommandResult.IsPublished);
				await AssertResponseMessageWasReceived(client, sendcommandResult);

				var device = client.Device(TestDeviceId);
				foreach (var filter in new[] { "limit", "page", "dir|desc", "dir|asc", "start", "end", "name", "status|pending", "status|processed", "status|rejected" })
				{
					M2XResponse result = null;
					switch (filter)
					{
						case "limit":
							result = await device.Commands("{ \"limit\": 2 }");
							break;
						case "page":
							result = await device.Commands("{ \"page\": 1 }");
							break;
						case "dir|desc":
							result = await device.Commands("{ \"dir\": \"desc\" }");
							break;
						case "dir|asc":
							result = await device.Commands("{ \"dir\": \"asc\" }");
							break;
						case "start":
							result = await device.Commands($"{{ \"start\": \"{DateTime.UtcNow.AddMinutes(-60).ToString(Constants.ISO8601_DateStartFormat)}\" }}");
							break;
						case "end":
							result = await device.Commands($"{{ \"end\": \"{DateTime.UtcNow.AddMinutes(-10).ToString(Constants.ISO8601_DateStartFormat)}\" }}");
							break;
						case "name":
							result = await device.Commands("{ \"name\": \"PHONE_HOME\" }");
							break;
						case "status|pending":
							result = await device.Commands("{ \"status\": \"pending\" }");
							break;
						case "status|processed":
							result = await device.Commands("{ \"status\": \"processed\" }");
							break;
						case "status|rejected":
							result = await device.Commands("{ \"status\": \"rejected\" }");
							break;
					}
					Assert.IsNotNull(result);
					Assert.IsTrue(result.IsPublished);
					await AssertResponseMessageWasReceived(client, result);
				}
			}
		}
	}

	public class ApiMqttTopicResponseWithCommands
	{
		public Command[] commands { get; set; }
	}

	public class Command
	{
		public string id { get; set; }
		public string url { get; set; }
		public string name { get; set; }
		public string status { get; set; }
		public DateTime sent_at { get; set; }
		public Status_Counts status_counts { get; set; }
	}

	public class Status_Counts
	{
		public int pending { get; set; }
		public int rejected { get; set; }
		public int processed { get; set; }
	}
}
