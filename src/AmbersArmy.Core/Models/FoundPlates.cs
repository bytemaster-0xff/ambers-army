using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbersArmy.Core.Models
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
}
