using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Utils
{
    class MediaFormatConvert
    {

        public static Dictionary<string, string> extToMimeType;
        public static Dictionary<string, string> mimeTypeToExt;

        static MediaFormatConvert()
        {

            extToMimeType = new Dictionary<string, string>();
            mimeTypeToExt = new Dictionary<string, string>();

            extToMimeType["tif"] = "image/tiff";
            extToMimeType["tiff"] = "image/tiff";
            extToMimeType["gif"] = "image/gif";
            extToMimeType["png"] = "image/png";
            extToMimeType["jpg"] = "image/jpeg";
            extToMimeType["jpeg"] = "image/jpeg";
            extToMimeType["bmp"] = "image/bmp";
            extToMimeType["asf"] = "video/x-ms-asf";
            extToMimeType["wmv"] = "video/x-ms-wmv";
            extToMimeType["flv"] = "video/x-flv";
            extToMimeType["mov"] = "video/quicktime";
            extToMimeType["mp4"] = "video/mp4";
            extToMimeType["avi"] = "video/avi";
            extToMimeType["mpg"] = "video/mpeg";
            extToMimeType["mpeg"] = "video/mpeg";
            extToMimeType["m4v"] = "video/x-m4v";
            extToMimeType["mkv"] = "video/x-matroska";

            foreach (KeyValuePair<string, string> pair in extToMimeType)
            {
                if (mimeTypeToExt.ContainsKey(pair.Value) == false)
                {
                    mimeTypeToExt.Add(pair.Value, pair.Key);
                }

            }

            mimeTypeToExt["video/vnd.avi"] = "avi";
            mimeTypeToExt["video/msvideo"] = "avi";
            mimeTypeToExt["video/x-msvideo"] = "avi";
            mimeTypeToExt["video/mpeg"] = "mpg";
            mimeTypeToExt["video/x-mpg"] = "mpg";
            mimeTypeToExt["video/mpeg2"] = "mpg";

        }

        public static string imageFormatToMimeType(ImageFormat imageFormat)
        {

            if (imageFormat == ImageFormat.Tiff)
            {
                return ("image/tiff");
            }
            else if (imageFormat == ImageFormat.Gif)
            {
                return ("image/gif");
            }
            else if (imageFormat == ImageFormat.Png)
            {
                return ("image/png");
            }
            else if (imageFormat == ImageFormat.Jpeg)
            {
                return ("image/jpeg");
            }
            else if (imageFormat == ImageFormat.Bmp)
            {
                return ("image/bmp");
            }
            else
            {
                return (null);
            }

        }

        public static ImageFormat mimeTypeToImageFormat(string mimeType)
        {

            if (mimeType.Equals("image/tiff"))
            {
                return (ImageFormat.Tiff);
            }
            else if (mimeType.Equals("image/gif"))
            {
                return (ImageFormat.Gif);
            }
            else if (mimeType.Equals("image/png"))
            {
                return (ImageFormat.Png);
            }
            else if (mimeType.Equals("image/jpeg"))
            {
                return (ImageFormat.Jpeg);
            }
            else if (mimeType.Equals("image/bmp"))
            {
                return (ImageFormat.Bmp);
            }
            else
            {
                return (null);
            }

        }

        public static ImageFormat fileNameToImageFormat(string fileName)
        {

            string ext = Path.GetExtension(fileName).ToLower();

            ImageFormat imageFormat;

            if (ext.Equals(".tif"))
            {
                imageFormat = ImageFormat.Tiff;
            }
            else if (ext.Equals(".gif"))
            {
                imageFormat = ImageFormat.Gif;
            }
            else if (ext.Equals(".png"))
            {
                imageFormat = ImageFormat.Png;
            }
            else if (ext.Equals(".bmp"))
            {
                imageFormat = ImageFormat.Bmp;
            }
            else
            {
                imageFormat = ImageFormat.Jpeg;
            }

            return (imageFormat);
        }

        public static string fileNameToMimeType(string fileName)
        {

            string ext = Path.GetExtension(fileName).ToLower().Replace(".", "");

            if (extToMimeType.ContainsKey(ext) == false)
            {
                return (null);
            }
            else
            {
                return (extToMimeType[ext]);
            }

        }

        public static string mimeTypeToExtension(string mimeType)
        {

            if (mimeTypeToExt.ContainsKey(mimeType) == false)
            {
                return (null);
            }
            else
            {
                return (mimeTypeToExt[mimeType]);
            }

        }

        public static bool isMediaFile(string fileName)
        {
            return (fileNameToMimeType(fileName) == null ? false : true);
        }

        public static bool isVideoFile(string fileName)
        {
            string mimeType = fileNameToMimeType(fileName);

            if (mimeType == null) return (false);
            else if (mimeType.StartsWith("video")) return (true);

            return (false);
        }

        public static bool isImageFile(string fileName)
        {
            string mimeType = fileNameToMimeType(fileName);

            if (mimeType == null) return (false);
            else if (mimeType.StartsWith("image")) return (true);

            return (false);
        }

    }
}
