using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace MediaViewer.MetaData
{

    public class MetaDataThumb 
    {

        private BitmapSource image;
        private MemoryStream data;
        private int width;
        private int height;
        private bool modified;

        public MetaDataThumb(BitmapSource image)
        {
            ThumbImage = image;
        }

        public MetaDataThumb(MemoryStream data)
        {
            this.data = data;

            BitmapImage tempImage = new BitmapImage();

            tempImage.BeginInit();
            tempImage.CacheOption = BitmapCacheOption.OnLoad;
            tempImage.StreamSource = data;
            tempImage.EndInit();

            image = tempImage;

            width = tempImage.PixelWidth;
            height = tempImage.PixelHeight;
            modified = false;
        }

        public BitmapSource ThumbImage
        {

            set
            {
                this.image = value;

                width = image.PixelWidth;
                height = image.PixelHeight;

                modified = true;
            }

            get
            {

                return (image);
            }
        }

        public int Width
        {

            get
            {

                return (width);
            }

            set
            {

                this.width = value;
            }
        }

        public int Height
        {

            get
            {

                return (height);
            }

            set
            {

                this.height = value;
            }
        }

        public MemoryStream Data
        {

            get
            {
                if (modified == true)
                {

                    data = new MemoryStream();

                    var encoder = new JpegBitmapEncoder(); 
                    encoder.Frames.Add(BitmapFrame.Create(image));

                    encoder.Save(data);                

                    modified = false;
                }

                return (data);
            }

            set
            {
                this.data = value;
            }
        }

 
    }
}
