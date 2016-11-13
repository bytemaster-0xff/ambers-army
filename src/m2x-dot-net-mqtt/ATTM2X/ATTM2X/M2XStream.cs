using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ATTM2X
{
	/// <summary>
	/// Wrapper for AT&T M2X Data Streams API
	/// https://m2x.att.com/developer/documentation/v2/device
	/// https://m2x.att.com/developer/documentation/v2/distribution
	/// </summary>
	public sealed class M2XStream : M2XClass
	{
		public const string UrlPath = "/streams";

		public readonly string StreamName;
		public readonly M2XDevice Device;
		public readonly M2XDistribution Distribution;

		private M2XStream(M2XClient client, string streamName)
			: base(client)
		{
			if (String.IsNullOrWhiteSpace(streamName))
				throw new ArgumentException(String.Format("Invalid streamName - {0}", streamName));

			this.StreamName = streamName;
		}
		internal M2XStream(M2XDevice device, string streamName)
			: this(device.Client, streamName)
		{
			this.Device = device;
		}
		internal M2XStream(M2XDistribution distribution, string streamName)
			: this(distribution.Client, streamName)
		{
			this.Distribution = distribution;
		}

		internal override string[] BuildPath(string[] path)
		{
			var pathToBuild = new List<string> { M2XStream.UrlPath, StreamName };
			pathToBuild.AddRange(path);

			return this.Device == null
				? this.Distribution.BuildPath(pathToBuild.ToArray())
				: this.Device.BuildPath(pathToBuild.ToArray());
		}

		public override Task<M2XResponse> Delete()
		{
			return MakeRequest(this.GetResourceIdentifiersWithIdIncluding(), M2XClientMethod.DELETE);
		}

		public override Task<M2XResponse> Update(object parms)
		{
			return MakeRequest(this.GetResourceIdentifiersWithIdIncluding(), M2XClientMethod.PUT, parms);
		}

		public override Task<M2XResponse> Details()
		{
			return MakeRequest(this.GetResourceIdentifiersWithIdIncluding());
		}

		/// <summary>
		/// Update the current value of the stream.
		///
		/// https://m2x.att.com/developer/documentation/v2/device#Update-Data-Stream-Value
		/// </summary>
		public Task<M2XResponse> UpdateValue(object parms)
		{
			var resourceParts = this.GetResourceIdentifiersWithIdIncluding(parms: new[] { "value" });
			return MakeRequest(resourceParts, M2XClientMethod.PUT, parms);
		}

		/// <summary>
		/// Post timestamped values to an existing data stream.
		///
		/// https://m2x.att.com/developer/documentation/v2/device#Post-Data-Stream-Values
		/// </summary>
		public Task<M2XResponse> PostValues(object parms)
		{
			var resourceParts = this.GetResourceIdentifiersWithIdIncluding(parms: new[] { "values" });
			return MakeRequest(resourceParts, M2XClientMethod.POST, parms);
		}

		/// <summary>
		/// Delete values in a stream by a date range
		///
		/// https://m2x.com/developer/documentation/v2/device#Delete-Data-Stream-Values
		/// </summary>
		public Task<M2XResponse> DeleteValues(object parms)
		{
			var resourceParts = this.GetResourceIdentifiersWithIdIncluding(parms: new[] { "values" });
			return MakeRequest(resourceParts, M2XClientMethod.DELETE, parms);
		}
	}
}