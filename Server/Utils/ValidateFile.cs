using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace Server.Utils
{
    public class ValidateFile
    {
        public static string GetImageExtention(byte[] fileBytes)
        {
            try
            {
                var format = Image.FromStream(new MemoryStream(fileBytes)).RawFormat;
                var extensions = ImageCodecInfo.GetImageEncoders().FirstOrDefault(encoder => encoder.FormatID == format.Guid).FilenameExtension;

                return extensions.Split(new[] { ';', '.', '*' }, StringSplitOptions.RemoveEmptyEntries)
                                 .First()
                                 .ToLower();
            }
            catch { }
            return null;
        }
    }
}