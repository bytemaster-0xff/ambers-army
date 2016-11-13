using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbersArmy.Core.Models
{
    public class Alert
    {
        public enum AlertTypes
        {
            Amber,
            Silver,
        }

        [JsonProperty("alertType")]
        public AlertTypes AlertType { get; set; }

        [JsonProperty("name")]
        public String Name { get; set;}
        [JsonProperty("age")]
        public String Age { get; set; }
        [JsonProperty("weight")]
        public String Weight { get; set; }
        [JsonProperty("birthDate")]
        public String BirthDate { get; set; }
        [JsonProperty("lastKnownLocationDateStamp")]
        public String LastKnowLocationDateStamp { get; set; }
        [JsonProperty("lastKnownLocation")]
        public GeoLocation LastKnownLocation { get; set; }
        [JsonProperty("licensePlate")]
        public String LicensePlate { get; set; }
        [JsonProperty("vehicleColor")]
        public String VehicleColor { get; set; }
        [JsonProperty("vehicleModel")]
        public String VehicleModel { get; set; }
        [JsonProperty("vehicleYear")]
        public String VehicleYear { get; set; }
    }
}
