using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATTM2X
{
	/// <summary>
	/// Wrapper for AT&T M2X Distribution API
	/// https://m2x.att.com/developer/documentation/v2/distribution
	/// </summary>
	public sealed class M2XDistribution : M2XClass
	{
		public const string UrlPath = "/distributions";

		public readonly string DistributionId;

		internal M2XDistribution(M2XClient client, string distributionId)
			: base(client)
		{
			if (String.IsNullOrWhiteSpace(distributionId))
				throw new ArgumentException(String.Format("Invalid distributionId - {0}", distributionId));

			this.DistributionId = distributionId;
		}

		internal override string[] BuildPath(string[] path)
		{
			var pathToBuild = new List<string> { M2XDistribution.UrlPath, DistributionId };
			pathToBuild.AddRange(path);
			return pathToBuild.ToArray();
		}

		/// <summary>
		/// Add a new device to an existing distribution
		///
		/// https://m2x.att.com/developer/documentation/v2/distribution#Add-Device-to-an-existing-Distribution
		/// </summary>
		public Task<M2XResponse> AddDevice(object parms)
		{
			var resourceParts = this.GetResourceIdentifiersWithIdIncluding(parms: new[] { M2XDevice.UrlPath });
			return MakeRequest(resourceParts, M2XClientMethod.POST, parms);
		}

	}
}