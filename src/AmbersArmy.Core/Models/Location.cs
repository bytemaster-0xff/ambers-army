using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbersArmy.Core.Models
{
    public class Location
    {
        public enum LocationType
        {
            Spotter,
        }

        [JsonProperty("timestamp")]
        public String TimeStamp { get; set; }

        [JsonProperty("location")]
        public GeoLocation Geo { get; set; }

        [JsonProperty("deviceId")]
        public String DeviceId { get; set; }

        [JsonProperty("locationType")]
        public LocationType Type { get; set; }
    }
}
