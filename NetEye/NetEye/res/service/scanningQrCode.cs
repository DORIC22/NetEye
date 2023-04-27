using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZXing.Mobile;
[assembly: Dependency(typeof(NetEye.res.service.scanningQrCode))]

namespace NetEye.res.service
{
    public class scanningQrCode
    {
        public async Task<string> ScanAsync()
        {
            var optionsDefault = new MobileBarcodeScanningOptions();
            var optionsCustom = new MobileBarcodeScanningOptions()
            {

                //UseFrontCameraIfAvailable = true,
                //Check diferents formats in http://barcode.tec-it.com/en
                // PossibleFormats = new List<ZXing.BarcodeFormat> {  ZXing.BarcodeFormat.CODE_128 }
            };               
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();
            scanner.BottomText = "Наведите камеру на QR или штрих код";
            scanner.TopText = "Нажмите для фокусировки";

            //await Flashlight.TurnOnAsync();
            var scanResults = await scanner.Scan(optionsDefault);
            //await Flashlight.TurnOffAsync();
            //Fix by Ale 2017-07-06
            return (scanResults != null) ? scanResults.Text : string.Empty;
        }
    }
}
