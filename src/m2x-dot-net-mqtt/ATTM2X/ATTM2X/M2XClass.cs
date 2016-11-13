using System;
using System.Net;
using System.Threading.Tasks;

namespace ATTM2X
{
	public abstract class M2XClass
	{
		public M2XClient Client { get; private set; }

		internal M2XClass(M2XClient client)
		{
			this.Client = client;
		}

		internal abstract string[] BuildPath(string[] path);

		public Task<M2XResponse> MakeRequest(string[] resourceIdentifiers = null, M2XClientMethod method = M2XClientMethod.GET, object parms = null)
		{
			return this.Client.MakeRequest(resourceIdentifiers, method, parms);
		}

		/// <summary>
		/// Get details of an existing entity.
		///
		/// https://m2x.att.com/developer/documentation/v2/device#View-Device-Details
		/// https://m2x.att.com/developer/documentation/v2/distribution#View-Distribution-Details
		/// https://m2x.att.com/developer/documentation/v2/keys#View-Key-Details
		/// https://m2x.att.com/developer/documentation/v2/device#View-Data-Stream
		/// https://m2x.att.com/developer/documentation/v2/distribution#View-Data-Stream
		/// </summary>
		public virtual Task<M2XResponse> Details()
		{
			return MakeRequest();
		}

		/// <summary>
		/// Update an existing entity.
		///
		/// https://m2x.att.com/developer/documentation/v2/device#Update-Device-Details
		/// https://m2x.att.com/developer/documentation/v2/distribution#Update-Distribution-Details
		/// https://m2x.att.com/developer/documentation/v2/keys#Update-Key
		/// https://m2x.att.com/developer/documentation/v2/device#Create-Update-Data-Stream
		/// https://m2x.att.com/developer/documentation/v2/distribution#Create-Update-Data-Stream
		/// </summary>
		public virtual Task<M2XResponse> Update(object parms)
		{
			return MakeRequest(new string[] { null }, M2XClientMethod.PUT, parms);
		}

		/// <summary>
		/// Delete an existing entity.
		///
		/// https://m2x.att.com/developer/documentation/v2/device#Delete-Device
		/// https://m2x.att.com/developer/documentation/v2/distribution#Delete-Distribution
		/// https://m2x.att.com/developer/documentation/v2/keys#Delete-Key
		/// https://m2x.att.com/developer/documentation/v2/device#Delete-Data-Stream
		/// https://m2x.att.com/developer/documentation/v2/distribution#Delete-Data-Stream
		/// </summary>
		public virtual Task<M2XResponse> Delete()
		{
			return MakeRequest(null, M2XClientMethod.DELETE);
		}
	}


	[Obsolete("This method is not supported by this library.", true)]
	public abstract class M2XClassWithMetadata : M2XClass
	{
		internal M2XClassWithMetadata(M2XClient client)
			: base(client)
		{
		}

		/// <summary>
		/// Get custom metadata of an existing entity.
		///
		/// https://m2x.att.com/developer/documentation/v2/device#Read-Device-Metadata
		/// https://m2x.att.com/developer/documentation/v2/distribution#Read-Distribution-Metadata
		/// https://m2x.att.com/developer/documentation/v2/collections#Read-Collection-Metadata
		/// </summary>
		public virtual Task<M2XResponse> Metadata()
		{
			return MakeRequest(new[] { "metadata" });
		}

		/// <summary>
		/// Update the custom metadata of the specified entity.
		///
		/// https://m2x.att.com/developer/documentation/v2/device#Update-Device-Metadata
		/// https://m2x.att.com/developer/documentation/v2/distribution#Update-Distribution-Metadata
		/// https://m2x.att.com/developer/documentation/v2/collections#Update-Collection-Metadata
		/// </summary>
		public virtual Task<M2XResponse> UpdateMetadata(object parms)
		{
			return MakeRequest(new[] { "metadata" }, M2XClientMethod.PUT, parms);
		}

		/// <summary>
		/// Get the value of a single custom metadata field from an existing entity.
		///
		/// https://m2x.att.com/developer/documentation/v2/device#Read-Device-Metadata-Field
		/// https://m2x.att.com/developer/documentation/v2/distribution#Read-Distribution-Metadata-Field
		/// https://m2x.att.com/developer/documentation/v2/collections#Read-Collection-Metadata-Field
		/// </summary>
		public virtual Task<M2XResponse> MetadataField(string field)
		{
			return MakeRequest(new[] { "metadata", field });
		}

		/// <summary>
		/// Update the custom metadata of the specified entity.
		///
		/// https://m2x.att.com/developer/documentation/v2/device#Update-Device-Metadata-Field
		/// https://m2x.att.com/developer/documentation/v2/distribution#Update-Distribution-Metadata-Field
		/// https://m2x.att.com/developer/documentation/v2/collections#Update-Collection-Metadata-Field
		/// </summary>
		public virtual Task<M2XResponse> UpdateMetadataField(string field, object parms)
		{
			return MakeRequest(new[] { "metadata", field }, M2XClientMethod.PUT, parms);
		}
	}
}
