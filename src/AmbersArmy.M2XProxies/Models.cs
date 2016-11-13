using Newtonsoft.Json;
using System;

namespace AmbersArmy.M2XProxies.Models
{
	public class FoundPlate
	{
		[JsonProperty("timestamp")]
		public String TimeStamp { get; set; }

		[JsonProperty("deviceId")]
		public String DeviceId { get; set; }

		[JsonProperty("plate")]
		public String Plate { get; set; }

		[JsonProperty("location")]
		public GeoLocation Location { get; set; }
	}

	public class GeoLocation
	{
		[JsonProperty("latitude")]
		public double Latitude { get; set; }

		[JsonProperty("longitude")]
		public double Longitude { get; set; }
	}
}