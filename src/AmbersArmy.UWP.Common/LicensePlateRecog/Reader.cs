using AmbersArmy.Core.Interfaces;
using AmbersArmy.Core.Models;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Ocr;
using System.Linq;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Devices.Enumeration;

namespace AmbersArmy.UWP.Common.LicensePlateRecog
{
    public class LicensePlateReader : IDisposable, ILicensePlateReader
    {
        MediaCapture _mediaCapture;
        OcrEngine _ocrEngine;


        public LicensePlateReader()
        {
            //    _mediaCapture = new MediaCapture();
            _ocrEngine = OcrEngine.TryCreateFromLanguage(new Language("en"));
        }

        public event EventHandler<OCRResult> TextRecognized;

        public void Dispose()
        {
            lock (this)
            {
                _mediaCapture.Dispose();
                _mediaCapture = null;
            }
        }

        public async Task InitAsync()
        {
            var videoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            var backCamera = videoDevices.FirstOrDefault(  item => item.EnclosureLocation != null && item.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Back);
           
            _mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings { VideoDeviceId = backCamera.Id };
            await _mediaCapture.InitializeAsync(settings);
            _mediaCapture.Failed += _mediaCapture_Failed;
        }

        public async Task ScanNow()
        {
            lock (this)
            {
                if (_mediaCapture == null)
                {
                    return;
                }
            }

            var start = DateTime.Now;
            Debug.WriteLine("Start >>");
            using (var captureStream = new InMemoryRandomAccessStream())
            {
                await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), captureStream);
                await DecodeRecognizeAndShow(captureStream);
            }

            var delta = DateTime.Now - start;
            Debug.WriteLine($"End => Elapsed {delta.TotalMilliseconds} ms");
        }

        private async Task DecodeRecognizeAndShow(IRandomAccessStream stream)
        {
            var decoder = await BitmapDecoder.CreateAsync(stream);
            using (var bitmap = await decoder.GetSoftwareBitmapAsync())
            {
                var result = await _ocrEngine.RecognizeAsync(bitmap);
                var lines = new List<OCRLine>();
                var ocrResult = new OCRResult()
                {
                    AllText = result.Text,
                    Lines = new List<OCRLine>()
                };

                Debug.WriteLine(result.Text);

                foreach (var item in result.Lines)
                {
                    lines.Add(new OCRLine()
                    {
                        Text = item.Text,
                        Words = (from wordItems
                                in item.Words
                                 select new OCRWord()
                                 {
                                     Text = wordItems.Text,
                                     BoundingBox = wordItems.BoundingRect
                                 }).ToList()
                    });
                }

                TextRecognized?.Invoke(this, ocrResult);
            }
        }


        private void _mediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            throw new NotImplementedException();
        }
    }
}
