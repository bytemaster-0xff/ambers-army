using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WIndowsPhoneClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
		}

		private OcrEngine ocrEngine;
		private async Task DecodeRecognizeAndShow(IRandomAccessStream stream, bool isVideo = false)
		{
			var decoder = await BitmapDecoder.CreateAsync(stream);

			if (!isVideo)
			{
				SoftwareBitmap bitmap = null;
				using (bitmap = await decoder.GetSoftwareBitmapAsync())
				{
					var result = await ocrEngine.RecognizeAsync(bitmap);
					//OcrResult.Text = !string.IsNullOrWhiteSpace(result.Text)
					//	? result.Text
					//	: "Recognition failed.  :'(";

					using (var convertedBitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied))
					{
						var convertedBitmapSource = new SoftwareBitmapSource();
						await convertedBitmapSource.SetBitmapAsync(convertedBitmap);
						//CaptureResult.Source = convertedBitmapSource;
					}
				}
			}
		}
	}
}
