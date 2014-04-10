using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoPlayerControl
{
    class Utils
    {
        public static void removeAndDispose<TypeName>(ref TypeName resource) where TypeName : class
        {
            if (resource == null)
                return;

            IDisposable disposer = resource as IDisposable;
            if (disposer != null)
            {
                try
                {
                    disposer.Dispose();
                }
                catch
                {
                }
            }

            resource = null;
        }

        public static T clamp<T>(T val, T min, T max) where T : IComparable
        {

            if (val.CompareTo(min) < 0) val = min;
            else if (val.CompareTo(max) > 0) val = max;

            return (val);
        }

        public static double lerp(double val, double min, double max)
        {

            val = clamp<double>(val, 0, 1);

            return ((1 - val) * min + val * max);
        }


        public static double invlerp(double val, double min, double max)
        {

            double result = (val - min) / (max - min);

            return (result);
        }

        public static string getPathWithoutFileName(string fullPath)
        {

            string fileName = System.IO.Path.GetFileName(fullPath);

            if (string.IsNullOrEmpty(fileName)) return (fullPath);

            return (fullPath.Remove(fullPath.Length - fileName.Length - 1));
        }

        public static string getUniqueFileName(string fileName)
        {

            string uniqueName = fileName;
            string dir = Path.GetDirectoryName(fileName);
            string name = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);

            int i = 0;

            while (File.Exists(uniqueName))
            {

                uniqueName = dir + "\\" + name + " (" + Convert.ToString(++i) + ")" + ext;

            }

            return (uniqueName);
        }

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
    }
}
