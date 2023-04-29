using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;
[assembly: Dependency(typeof(NetEye.res.service.scanningQrCode))]

namespace NetEye.res.service
{
    public class scanningQrCode
    {
        public async Task<string> ScanAsync()
        {            
            var optionsDefault = new MobileBarcodeScanningOptions();            
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();           
            scanner.BottomText = "Наведите камеру на QR или штрих код";
            scanner.TopText = "Нажмите для фокусировки";
            scanner.Torch(true);
            var scanResults = await scanner.Scan(optionsDefault);
            return (scanResults != null) ? scanResults.Text : string.Empty;
        }             
    }
}
