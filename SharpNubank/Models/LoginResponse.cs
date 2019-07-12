using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SharpNubank.Models
{
    public class LoginResponse
    {
        public string Token { get; set; }

        public bool NeedsDeviceAuthorization { get; set; }

        public string Code { get; set; }

        public string GetQrCodeAsAscii()
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(Code, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new AsciiQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(1);
                }
                
            } 
        }

        //public Bitmap GetQrCodeAsBitmap()
        //{
        //    using (var qrGenerator = new QRCodeGenerator())
        //    {
        //        var qrCodeData = qrGenerator.CreateQrCode(Code, QRCodeGenerator.ECCLevel.Q);
        //        using (var qrCode = new QRCode(qrCodeData))
        //        { 
        //            return qrCode.GetGraphic(20);
        //        }
        //    }
        //}
    }
}
