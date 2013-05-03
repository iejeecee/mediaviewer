using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace MediaViewer.MetaData
{

    class MetaDataThumb : IDisposable
    {

        private Image image;
        private MemoryStream data;
        private int width;
        private int height;
        private bool modified;

        public MetaDataThumb(Image image)
        {
            ThumbImage = image;
        }

        public MetaDataThumb(MemoryStream data)
        {

            this.data = data;
            image = new Bitmap(data);
            width = image.Width;
            height = image.Height;
            modified = false;
        }

        public Image ThumbImage
        {

            set
            {
                this.image = value;

                width = image.Width;
                height = image.Height;

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

                    image.Save(data, System.Drawing.Imaging.ImageFormat.Jpeg);

                    modified = false;
                }

                return (data);
            }

            set
            {
                this.data = value;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (image != null)
            {
                image.Dispose();
            }
        }

        #endregion
    }
}
