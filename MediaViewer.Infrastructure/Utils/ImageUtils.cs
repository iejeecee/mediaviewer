using MediaViewer.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MediaViewer.Infrastructure.Utils
{
    public class ImageUtils
    {        
        public static Rectangle centerRectangle(Rectangle outer, Rectangle inner)
        {

            Rectangle center = new Rectangle(outer.X, outer.Y, inner.Width, inner.Height);

            center.X += (outer.Width - inner.Width) / 2;
            center.Y += (outer.Height - inner.Height) / 2;

            return (center);
        }


        public static RectangleF centerRectangle(RectangleF outer, RectangleF inner)
        {

            RectangleF center = new RectangleF(outer.X, outer.Y, inner.Width, inner.Height);

            center.X += (outer.Width - inner.Width) / 2;
            center.Y += (outer.Height - inner.Height) / 2;

            return (center);
        }

        /// <summary>
        /// returns scale required to scale input to desired size
        /// </summary>
        /// <param name="width">input width</param>
        /// <param name="height">input height</param>
        /// <param name="maxWidth">maximum width</param>
        /// <param name="maxHeight">maximum height</param>
        /// <returns></returns>
        public static float resizeRectangle(int width, int height, int maxWidth, int maxHeight)
        {
            float widthScale = 1;
            float heightScale = 1;

            if (width > maxWidth)
            {

                widthScale = maxWidth / (float)width;
            }

            if (height > maxHeight)
            {

                heightScale = maxHeight / (float)height;

            }

            return (Math.Min(widthScale, heightScale));

        }

        public static void resizeRectangle(int width, int height, int maxWidth, int maxHeight, out int scaledWidth, out int scaledHeight)
        {
            float scale = resizeRectangle(width, height, maxWidth, maxHeight);

            scaledWidth = (int)(Math.Round(width * scale));
            scaledHeight = (int)(Math.Round(height * scale));
        }

        public static Rectangle stretchRectangle(Rectangle rec, Rectangle max)
        {

            float widthScale = 1;
            float heightScale = 1;

            widthScale = max.Width / (float)rec.Width;
            heightScale = max.Height / (float)rec.Height;

            Rectangle stretched = new Rectangle(rec.X, rec.Y, 0, 0);

            stretched.Width = (int)(rec.Width * Math.Min(widthScale, heightScale));
            stretched.Height = (int)(rec.Height * Math.Min(widthScale, heightScale));

            if (stretched.Width > max.Width) stretched.Width = max.Width;
            if (stretched.Height > max.Height) stretched.Height = max.Height;

            return (stretched);
        }

        public static Image resizeImage(Image source, int width, int height)
        {

            Image result = null;

            try
            {

                if (source.Width == width && source.Height == height)
                {

                    return (new Bitmap(source));
                }

                result = new Bitmap(width, height, source.PixelFormat);
                Graphics g = Graphics.FromImage(result);

                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.DrawImage(source, new Rectangle(0, 0, width, height));

            }
            catch (Exception e)
            {

                Logger.Log.Error("Error resizing image", e);
                //MessageBox.Show(e.Message, "Error resizing image");

            }

            return (result);

        }

        public static Image createImageFromArray(int width, int height,
            System.Drawing.Imaging.PixelFormat format, byte[] data)
        {

            Bitmap bitmap = new Bitmap(width, height, format);

            Rectangle rect = new Rectangle(0, 0, width, height);

            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly,
                format);

            IntPtr ptr = bmpData.Scan0;

            Marshal.Copy(data, 0, ptr, data.Length);

            bitmap.UnlockBits(bmpData);

            return (bitmap);
        }

        public static Rotation getBitmapRotation(Stream data)
        {
            BitmapDecoder decoder = BitmapDecoder.Create(data,
                    BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);

            BitmapMetadata meta = decoder.Frames[0].Metadata as BitmapMetadata;

            Rotation? image = ImageUtils.getOrientationFromMetatdata(meta);

            if (image != null)
            {
                return (image.Value);
            }
            else
            {
                return (Rotation.Rotate0);
            }

        }

        public static bool isUrl(string path)
        {
            Uri uriResult;

            bool isUrl = Uri.TryCreate(path, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;

            return (isUrl);
        }

        public static BitmapImage loadImage(string location, CancellationToken token)
        {

            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }

            BitmapImage loadedImage = new BitmapImage();
            Stream imageData = null;

            try
            {
                if (isUrl(location))
                {
                    imageData = new MemoryStream();
                    String mimeType;

                    StreamUtils.readHttpRequest(new Uri(location), imageData, out mimeType, token);
                }
                else
                {
                    imageData = File.Open(location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }

                Rotation rotation = ImageUtils.getBitmapRotation(imageData);
                imageData.Position = 0;

                loadedImage.BeginInit();
                loadedImage.CacheOption = BitmapCacheOption.OnLoad;
                loadedImage.StreamSource = imageData;
                loadedImage.Rotation = rotation;
                loadedImage.EndInit();

                loadedImage.Freeze();

                Logger.Log.Info("Image loaded: " + location);

                return (loadedImage);
            }
            finally
            {
                if (imageData != null)
                {
                    imageData.Close();
                }
            }
        }
      

        public static Rotation? getOrientationFromMetatdata(BitmapMetadata meta)
        {
            if (meta != null)
            {
                UInt16 orientation = 1;
                Object result = meta.GetQuery("/app1/ifd/{ushort=274}");

                if (result != null)
                {
                    orientation = (UInt16)result;
                }
                else
                {
                    return (null);
                }

                switch (orientation)
                {
                    case 8:
                        return (Rotation.Rotate270);
                    case 3:
                        return (Rotation.Rotate180);
                    case 6:
                        return (Rotation.Rotate90);
                    default:
                        return (Rotation.Rotate0);

                }

            }
            else
            {
                return (null);
            }
        }

        public static BitmapSource jpegBase64StringToImage(String encoded)
        {
            if (String.IsNullOrEmpty(encoded)) return (null);

            byte[] bytes = System.Convert.FromBase64String(encoded);

            MemoryStream stream = new MemoryStream(bytes);

            JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.None);
            BitmapFrame image = decoder.Frames[0];
            image.Freeze();

            return (image);
        }


        public static string imageToJpegBase64String(BitmapSource image)
        {
            if (image == null) return (null);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.QualityLevel = 100;
            byte[] bytes = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
                bytes = stream.ToArray();
                stream.Close();
            }

            return (System.Convert.ToBase64String(bytes));
        }
    }
}
