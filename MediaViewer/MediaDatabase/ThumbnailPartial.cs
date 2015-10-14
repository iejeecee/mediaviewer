using MediaViewer.Model.Media.File.Watcher;
using Microsoft.Practices.Prism.Mvvm;
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
    partial class Thumbnail : BindableBase, IEquatable<Thumbnail>
    {
        [NonSerialized]
        BitmapSource image;
        
        public BitmapSource Image
        {
            get { return image; } 
            private set {
               
                SetProperty(ref image, value);
            }
        }

        public Thumbnail()
        {
            Image = null;
        }
      
        public Thumbnail(BitmapSource source, double? timeSeconds = null)
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

            TimeSeconds = timeSeconds;
           
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

        public bool Equals(Thumbnail other)
        {
            if (other == null)
            {
                throw new InvalidOperationException();
            }

            if (other.Id == Id) return (true);
            else return (false);
        }
    }
}
