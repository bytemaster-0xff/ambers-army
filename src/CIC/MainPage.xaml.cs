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
using Windows.Services.Maps;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CIC
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		M2XService svc;

		public MainPage()
		{
			this.InitializeComponent();
			svc = new M2XService();
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			await svc.SetupMqttClient();

			var result = await svc.GetDeviceStream("6e7bb7923219c6b72728cb1a34d0d5b6", "Temp");

			var foo = result;
			var fooCheck = string.IsNullOrWhiteSpace(result);
			M2xResult.Text = result;
		}
	}
}