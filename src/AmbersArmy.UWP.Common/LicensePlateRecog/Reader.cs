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

namespace AmbersArmy.UWP.Common.LicensePlateRecog
{
    public class Reader : ILicensePlateReader
    {
        MediaCapture _mediaCapture;
        OcrEngine _ocrEngine;


        public Reader()
        {
            _mediaCapture = new MediaCapture();
            _ocrEngine = OcrEngine.TryCreateFromLanguage(new Language("en"));
        }

        public event EventHandler<OCRResult> TextRecognized;

        public async Task InitAsync()
        {
            _mediaCapture = new MediaCapture();
            await _mediaCapture.InitializeAsync();
            _mediaCapture.Failed += _mediaCapture_Failed;
        }

        public async void TakePix()
        {
            var myPictures = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
            var file = await myPictures.SaveFolder.CreateFileAsync("photo.jpg", CreationCollisionOption.GenerateUniqueName);

            using (var captureStream = new InMemoryRandomAccessStream())
            {
                await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), captureStream);
                await DecodeRecognizeAndShow(captureStream);
            }
        }

        private async Task DecodeRecognizeAndShow(IRandomAccessStream stream)
        {
            var decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap bitmap = null;
            using (bitmap = await decoder.GetSoftwareBitmapAsync())
            {
                var result = await _ocrEngine.RecognizeAsync(bitmap);
                var lines = new List<OCRLine>();


                var ocrResult = new OCRResult()
                {
                    AllText = result.Text,
                    Lines = new List<OCRLine>()
                };

                foreach(var item in result.Lines)
                {
                    lines.Add(new OCRLine()
                    {
                         Text = item.Text,
                         Words = (from wordItems 
                                 in item.Words
                                 select new OCRWord()
                                 {
                                     Text = wordItems.Text,
                                     BoundingBox =  wordItems.BoundingRect
                                 }).ToList()
                    });
                }

                TextRecognized?.Invoke(this, ocrResult);
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


        private void _mediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            throw new NotImplementedException();
        }
    }
}
