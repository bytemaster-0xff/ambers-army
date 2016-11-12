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


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CIC
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int idx = 0;
        public MainPage()
        {
            this.InitializeComponent();
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
            TheMap.ZoomLevel = 14;
            TheMap.Tapped += TheMap_Tapped;

            TheMap.MapElementClick += TheMap_MapElementClick;
            TheMap.Tapped += TheMap_Tapped1;

            var startLocation = new BasicGeoposition() { Latitude = 33.717486, Longitude = -84.398336 };

            // End at the city of Seattle, Washington.
            var endLocation = new BasicGeoposition() { Latitude = 33.748577, Longitude = -84.441310 };

            // Get the route between the points.
            var  routeResult =
                  await MapRouteFinder.GetDrivingRouteAsync(
                  new Geopoint(startLocation),
                  new Geopoint(endLocation),
                  MapRouteOptimization.Time,
                  MapRouteRestrictions.None);

            foreach(var item in routeResult.Route.Legs)
            {
                foreach(var path in item.Path.Positions)
                {
                    Debug.WriteLine($"{{new SampleGeoPos() {{Idx ={idx++},Lat={path.Latitude},Lon={path.Longitude}}}}},");
                }
            }

        }

        private void TheMap_Tapped1(object sender, TappedRoutedEventArgs e)
        {
           
        }

        private void TheMap_MapElementClick(Windows.UI.Xaml.Controls.Maps.MapControl sender, Windows.UI.Xaml.Controls.Maps.MapElementClickEventArgs args)
        {
            Debug.WriteLine($"{{new SampleGeoPos() {{Idx ={idx++},Lat={args.Location.Position.Latitude},Lon={args.Location.Position.Longitude}}}}}}}"); 
        }

        private void TheMap_Tapped(object sender, TappedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}