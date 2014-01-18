using MediaViewer.MediaFileModel.Watcher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MediaViewer.MediaDatabase
{
    partial class Thumbnail
    {
        public Thumbnail(BitmapSource source)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            BitmapFrame outputFrame = BitmapFrame.Create(source, null, null, null);
            encoder.Frames.Add(outputFrame);
            encoder.QualityLevel = 80;

            MemoryStream stream = new MemoryStream();

            encoder.Save(stream);

            this.ImageData = stream.ToArray();
            this.Width = (short)source.PixelWidth;
            this.Height = (short)source.PixelHeight;
        }

        public BitmapSource toBitmapSource()
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(ImageData, 0, ImageData.Length);
            stream.Position = 0;

            BitmapImage tempImage = new BitmapImage();
            tempImage.BeginInit();
            tempImage.CacheOption = BitmapCacheOption.OnLoad;
            tempImage.StreamSource = stream;
            tempImage.DecodePixelWidth = Width;
            tempImage.DecodePixelHeight = Height;
            tempImage.EndInit();

            return (tempImage);
        }
    }
}
