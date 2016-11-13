using AmbersArmy.Core.Interfaces;
using AmbersArmy.Core.Models;
using AmbersArmy.Core.Services;
using AmbersArmy.Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AmbersArmy.Core.ViewModels
{
    public class Spotter
    {
        FlowClient _flowClient = new FlowClient();
        Interfaces.ILicensePlateReader _plateReader;
        Interfaces.ITimer _timer;
        private ObservableCollection<String> _recognizedTags;

        public Spotter()
        {
            PostLocationCommand = new RelayCommand(PostLocation);
            CurrentLocation = new GeoLocation()
            {
                Latitude = 36.99,
                Longitude = -86.0
            };


            _plateReader = AAIOC.Get<Interfaces.ILicensePlateReader>();
            _plateReader.TextRecognized += _plateReader_TextRecognized;
            _recognizedTags = new ObservableCollection<string>();
        }

        public async Task InitAsync()
        {
            Debug.WriteLine("INITTING READER");
            await _plateReader.InitAsync();
            _timer = AAIOC.Get<ITimerFactory>().CreateTimer(TimeSpan.FromSeconds(1));
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
            Debug.WriteLine("WE INIT SO START TIMER!");
        }

        private void _plateReader_TextRecognized(object sender, OCRResult e)
        {
            if (String.IsNullOrEmpty(e.AllText))
            {
                Debug.WriteLine("MISSING");
            }
            else
            {
                _flowClient.PostLicensePlateAysnc(new Models.FoundPlate()
                {
                    Plate = e.AllText,
                    DeviceId = SpotterId,
                    TimeStamp = DateTime.Now.ToJSONString(),
                    Location = new GeoLocation() { Latitude = 34.3, Longitude = 33.3 }
                });

                Debug.WriteLine($"VALID => {e.AllText}");
            }
        }

        private async void _timer_Elapsed(object sender, EventArgs e)
        {
            _timer.Stop();
            await _plateReader.ScanNow();
            _timer.Start();
        }

        public GeoLocation CurrentLocation { get; set; }

        public String SpotterId { get; set; }

        public async void PostLocation()
        {
            var location = new Models.Location()
            {
                DeviceId = SpotterId,
                Type = Location.LocationType.Spotter,
                Geo = CurrentLocation,
                TimeStamp = DateTime.Now.ToJSONString()
            };

            await _flowClient.PostLocationAsync(location);
        }

        public ICommand PostLocationCommand { get; private set; }

        public ObservableCollection<string> RecognizedTags { get { return _recognizedTags; } }
    }
}
