using MediaViewer.MediaDatabase;
using MediaViewer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MediaViewer.MediaFileModel
{
    class ImageMetadataReader : MetadataReader
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void readMetadata(Stream data, MediaFactory.ReadOptions options, Media media)
        {
            ImageMedia image = media as ImageMedia;

            BitmapDecoder bitmapDecoder = null;
         
            try
            {
                bitmapDecoder = BitmapDecoder.Create(data,
                  BitmapCreateOptions.DelayCreation,
                  BitmapCacheOption.OnDemand);

                BitmapFrame frame = bitmapDecoder.Frames[0];

                image.Width = frame.PixelWidth;
                image.Height = frame.PixelHeight;

                if (options.HasFlag(MediaFactory.ReadOptions.GENERATE_THUMBNAIL))
                {
                    generateThumbnail(data, frame, image);
                }

            }
            catch (Exception e)
            {
                log.Error("Cannot generate image thumbnail: " + image.Location, e);
                media.MetadataReadError = e;
            }
            finally
            {
                data.Position = 0;
            }

            if (!FileUtils.isUrl(image.Location))
            {
                image.SupportsXMPMetadata = true;

                base.readMetadata(data, options, media);
            }
            else
            {
                image.SupportsXMPMetadata = false;
            }
          
        }

        public void generateThumbnail(Stream data, BitmapFrame frame, ImageMedia image)
        {

            BitmapSource thumb = frame.Thumbnail;

            if (thumb == null)
            {
                data.Position = 0;

                var tempImage = new BitmapImage();
                tempImage.BeginInit();
                tempImage.CacheOption = BitmapCacheOption.OnLoad;
                tempImage.StreamSource = data;
                tempImage.DecodePixelWidth = MAX_THUMBNAIL_WIDTH;
                tempImage.EndInit();

                thumb = tempImage;
            }

            image.Thumbnail = new Thumbnail(thumb);

        }



    }
}
