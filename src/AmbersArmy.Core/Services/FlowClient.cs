using AmbersArmy.Core.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace AmbersArmy.Core.Services
{
    public class FlowClient
    {
        HttpClient _client = new HttpClient();

        public Task PostLicensePlateAysnc(FoundPlate foundPlate)
        {
            var uri = "https://run-west.att.io/3f7a4e0dfd560/257e916b1900/6a423dcbc2f47b1/in/flow/plates";
            var json = JsonConvert.SerializeObject(foundPlate);
            return _client.PostAsync(uri, new StringContent(json));
        }

        public  Task PostLocationAsync(Models.Location location)
        {
            var uri = "https://run-west.att.io/3f7a4e0dfd560/257e916b1900/6a423dcbc2f47b1/in/flow/location";
            var json = JsonConvert.SerializeObject(location);
            return _client.PostAsync(uri, new StringContent(json));
        }

        public  Task PostAlertAsync(Models.Alert alert)
        {
            var uri = "https://run-west.att.io/3f7a4e0dfd560/257e916b1900/6a423dcbc2f47b1/in/flow/ingestalert";
            var json = JsonConvert.SerializeObject(alert);
            return _client.PostAsync(uri, new StringContent(json));
        }
    }
}
