using MediaViewer.MediaFileModel.Watcher;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MediaViewer.MediaDatabase
{
    [Serializable]
    partial class Thumbnail : ObservableObject
    {
        [NonSerialized]
        BitmapSource image;
        
        public BitmapSource Image
        {
            get { return image; } 
            private set {
                image = value;
                NotifyPropertyChanged();
            }
        }

        public Thumbnail()
        {

        }

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

            source.Freeze();           
            Image = source;
           
        }

        public void decodeImage()
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(ImageData, 0, ImageData.Length);
            stream.Position = 0;

            BitmapImage thumb = new BitmapImage();
            thumb.BeginInit();
            thumb.CacheOption = BitmapCacheOption.OnLoad;
            thumb.StreamSource = stream;
            thumb.DecodePixelWidth = Width;
            thumb.DecodePixelHeight = Height;
            thumb.EndInit();

            thumb.Freeze();

            Image = thumb;
           
        }

     
    }
}
