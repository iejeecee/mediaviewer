using MediaViewer.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaViewer.Model.Utils
{
    class ImageUtils
    {

        


        public static Rectangle centerRectangle(Rectangle outer, Rectangle inner)
        {

            Rectangle center = new Rectangle(outer.X, outer.Y, inner.Width, inner.Height);

            center.X += (outer.Width - inner.Width) / 2;
            center.Y += (outer.Height - inner.Height) / 2;

            return (center);
        }

        public static void resizeRectangle(int width, int height, int maxWidth, int maxHeight, out int scaledWidth, out int scaledHeight)
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

            scaledWidth = (int)(Math.Round(width * Math.Min(widthScale, heightScale)));
            scaledHeight = (int)(Math.Round(height * Math.Min(widthScale, heightScale)));
        }

        public static Rectangle stretchRectangle(Rectangle rec, Rectangle max)
        {

            float widthScale = 1;
            float heightScale = 1;

            widthScale = max.Width / (float)rec.Width;
            heightScale = max.Height / (float)rec.Height;

            Rectangle stretched = new Rectangle(rec.X, rec.Y, 0, 0);

            stretched.Width = (int)(Math.Round(rec.Width * Math.Min(widthScale, heightScale)));
            stretched.Height = (int)(Math.Round(rec.Height * Math.Min(widthScale, heightScale)));

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
                MessageBox.Show(e.Message, "Error resizing image");

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
    }
}
