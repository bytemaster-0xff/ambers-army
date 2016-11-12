using AmbersArmy.Core.Models;
using System;
using System.Threading.Tasks;

namespace AmbersArmy.Core.Interfaces
{
    public interface ILicensePlateReader
    {
        event EventHandler<OCRResult> TextRecognized;
        Task InitAsync();
    }
}
