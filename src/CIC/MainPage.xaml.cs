using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using AmbersArmy.Core.Services;
using System.Diagnostics;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using AmbersArmy.Core.RouteSimulator;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Storage.Streams;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CIC
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int idx = 0;
        List<SampleVehicle> _sampleVehicles = new List<SampleVehicle>();
        Dictionary<int, MapIcon> _carIcons = new Dictionary<int, MapIcon>();
        MapPolygon _cicleOfHope;
        int _radius = 4000;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Timer_Tick(object sender, object e)
        {
            foreach (var vehicle in _sampleVehicles)
            {
                vehicle.Update();
            }

            _radius += 100;

            _cicleOfHope.Path = new Geopath(TheMap.Center.GetCirclePoints(_radius));
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var svc = new M2XService();
            var result = await svc.GetDeviceStream("6e7bb7923219c6b72728cb1a34d0d5b6", "Temp");

            var foo = result;
            var fooCheck = string.IsNullOrWhiteSpace(result);
            M2xResult.Text = result;

            TheMap.Center = new Windows.Devices.Geolocation.Geopoint(new Windows.Devices.Geolocation.BasicGeoposition()
            {
                Latitude = 33.7490,
                Longitude = -84.3880
            });
            TheMap.ZoomLevel = 13;

            AddCars();
            AddCircleOfHope();

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void AddCars()
        {
            for (var idx = 0; idx < 20; ++idx)
            {
                var car = SampleVehicle.Create(idx);
                car.LocationChanged += Car_LocationChanged;
                _sampleVehicles.Add(car);
                var icon = new MapIcon();
                icon.NormalizedAnchorPoint = new Point(0.5, 1);
                icon.Title = (idx + 1).ToString();
                icon.ZIndex = 0;
                icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/soldier_avatar.png"));
                icon.Location = new Geopoint(new BasicGeoposition() { Latitude = car.CurrentLocation.Lat, Longitude = car.CurrentLocation.Lon });
                TheMap.MapElements.Add(icon);
                _carIcons.Add(idx, icon);
            }
        }

        private void AddCircleOfHope()
        {
            _cicleOfHope = new MapPolygon()
            {
                FillColor = Windows.UI.Color.FromArgb(0x50, 0xFF, 0x00, 0x00)
            };

            _cicleOfHope.ZIndex = 0;
            _cicleOfHope.Path = new Geopath(TheMap.Center.GetCirclePoints(_radius));
            TheMap.MapElements.Add(_cicleOfHope);
        }

        private void Car_LocationChanged(object sender, EventArgs e)
        {
            var car = sender as SampleVehicle;
            var mapIcon = _carIcons[car.CarIndex];
            mapIcon.Location = new Geopoint(new BasicGeoposition() { Latitude = car.CurrentLocation.Lat, Longitude = car.CurrentLocation.Lon });
        }

        private async void GenerateRoute()
        {
            var startLocation = new BasicGeoposition() { Latitude = 33.717486, Longitude = -84.398336 };
            // End at the city of Seattle, Washington.
            var endLocation = new BasicGeoposition() { Latitude = 33.748577, Longitude = -84.441310 };

            // Get the route between the points.
            var routeResult =
                  await MapRouteFinder.GetDrivingRouteAsync(
                  new Geopoint(startLocation),
                  new Geopoint(endLocation),
                  MapRouteOptimization.Time,
                  MapRouteRestrictions.None);

            foreach (var item in routeResult.Route.Legs)
            {
                foreach (var path in item.Path.Positions)
                {
                    Debug.WriteLine($"{{new SampleGeoPos() {{Idx ={idx++},Lat={path.Latitude},Lon={path.Longitude}}}}},");
                }
            }

        }
    }
}