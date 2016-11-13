using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ATTM2X
{
	/// <summary>
	/// Wrapper for AT&T M2X Device API
	/// https://m2x.att.com/developer/documentation/v2/device
	/// </summary>
	public sealed class M2XDevice : M2XClass
	{
		public const string UrlPath = "/devices";

		public readonly string DeviceId;

		internal M2XDevice(M2XClient client, string deviceId)
			: base(client)
		{
			if (string.IsNullOrWhiteSpace(deviceId))
				throw new ArgumentException(string.Format("Invalid deviceId - {0}", deviceId));

			this.DeviceId = deviceId;
		}

		internal override string[] BuildPath(string[] path)
		{
			var pathToBuild = new List<string> { M2XDevice.UrlPath };
			pathToBuild.Add(DeviceId);
			pathToBuild.AddRange(path);
			return pathToBuild.ToArray();
		}

		public override Task<M2XResponse> Delete()
		{
			return MakeRequest(this.GetResourceIdentifiersWithIdIncluding(), M2XClientMethod.DELETE);
		}

		public override Task<M2XResponse> Details()
		{
			return MakeRequest(this.GetResourceIdentifiersWithIdIncluding());
		}

		public override Task<M2XResponse> Update(object parms)
		{
			return MakeRequest(this.GetResourceIdentifiersWithIdIncluding(), M2XClientMethod.PUT, parms);
		}

		/// <summary>
		/// Retrieve the list of recent commands sent to the current device (as given by the API key).
		///
		/// https://m2x.att.com/developer/documentation/v2/commands#Device-s-List-of-Received-Commands
		/// </summary>
		public Task<M2XResponse> Commands(object parms = null)
		{
			var resourceParts = this.GetResourceIdentifiersWithIdIncluding(parms: new[] { "commands" });
			return MakeRequest(resourceParts, M2XClientMethod.GET, parms);
		}

		/// <summary>
		/// Update the current location of the specified device.
		///
		/// https://m2x.att.com/developer/documentation/v2/device#Read-Device-Location
		/// </summary>
		public Task<M2XResponse> UpdateLocation(object parms)
		{
			var resourceParts = this.GetResourceIdentifiersWithIdIncluding(parms: "location");
			return MakeRequest(resourceParts, M2XClientMethod.PUT, parms);
		}

		/// <summary>
		/// Get a wrapper to access a data stream associated with the specified Device.
		/// </summary>
		public M2XStream Stream(string streamName)
		{
			return new M2XStream(this, streamName);
		}

		/// <summary>
		/// Posts single values to multiple streams at once.
		///
		/// https://m2x.att.com/developer/documentation/v2/device#Post-Device-Update--Single-Values-to-Multiple-Streams-
		/// </summary>
		public Task<M2XResponse> PostUpdate(object parms)
		{
			var resourceParts = this.GetResourceIdentifiersWithIdIncluding(parms: new[] { "update" });
			return MakeRequest(resourceParts, M2XClientMethod.POST, parms);
		}

		/// <summary>
		/// Post values to multiple streams at once.
		///
		/// https://m2x.att.com/developer/documentation/v2/device#Post-Device-Updates--Multiple-Values-to-Multiple-Streams-
		/// </summary>
		public Task<M2XResponse> PostUpdates(object parms)
		{
			var resourceParts = this.GetResourceIdentifiersWithIdIncluding(parms: new[] { "updates" });
			return MakeRequest(resourceParts, M2XClientMethod.POST, parms);
		}

		/// <summary>
		/// Mark the given command as processed by the device, changing the status from "sent" to "processed".
		///
		/// https://m2x.att.com/developer/documentation/v2/commands#Device-Marks-a-Command-as-Processed
		/// </summary>
		public Task<M2XResponse> ProcessCommand(string commandId, object parms = null)
		{
			var resourceParts = this.GetResourceIdentifiersWithIdIncluding(parms: new[] { "commands", commandId, "process" });
			return MakeRequest(resourceParts, M2XClientMethod.POST, parms);
		}

		/// <summary>
		/// Mark the given command as rejected by the device, changing the status from "sent" to "rejected".
		///
		/// https://m2x.att.com/developer/documentation/v2/commands#Device-Marks-a-Command-as-Rejected
		/// </summary>
		public Task<M2XResponse> RejectCommand(string commandId, object parms = null)
		{
			var resourceParts = this.GetResourceIdentifiersWithIdIncluding(parms: new[] { "commands", commandId, "reject" });
			return MakeRequest(resourceParts, M2XClientMethod.POST, parms);
		}
	}
}