using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GoogleImageSearchPlugin
{
    class ImageResultItem : MediaItem
    {
        public Bing.ImageResult ImageInfo { protected set; get; }

        static String titleToFilename(String title, Bing.ImageResult imageResult)
        {
            String filename = FileUtils.removeIllegalCharsFromFileName(title, " ");

            String ext = "." + MediaFormatConvert.mimeTypeToExtension(imageResult.ContentType);

            if (!filename.EndsWith(ext, true, CultureInfo.InvariantCulture))
            {
                filename += ext;
            }

            return (filename);
        }


        public ImageResultItem(Bing.ImageResult imageResult) :
            base(imageResult.Title,MediaItemState.EMPTY)//base(titleToFilename(imageResult.Title, imageResult), MediaItemState.EMPTY)
        {            
            ImageInfo = imageResult;
        }

        public override void readMetadata(MediaViewer.Model.metadata.Metadata.MetadataFactory.ReadOptions options, System.Threading.CancellationToken token)
        {
            MemoryStream data = new MemoryStream();
            String mimeType;

            RWLock.EnterUpgradeableReadLock();
            try
            {
                ItemState = MediaItemState.LOADING;

                StreamUtils.download(new Uri(ImageInfo.Thumbnail.MediaUrl), data, out mimeType, token);

                BitmapDecoder decoder = BitmapDecoder.Create(data,
                                    BitmapCreateOptions.PreservePixelFormat,
                                    BitmapCacheOption.OnLoad);
                BitmapSource bitmapSource = decoder.Frames[0];
                bitmapSource.Freeze();

                ImageMetadata metaData = new ImageMetadata();
                metaData.Thumbnail = new Thumbnail(bitmapSource);

                metaData.Location = ImageInfo.MediaUrl;
                metaData.Width = ImageInfo.Width.HasValue ? ImageInfo.Width.Value : 0;
                metaData.Height = ImageInfo.Height.HasValue ? ImageInfo.Height.Value : 0;
                metaData.SizeBytes = ImageInfo.FileSize.HasValue ? ImageInfo.FileSize.Value : 0;
                metaData.MimeType = ImageInfo.ContentType;

                Metadata = metaData;

                ItemState = MediaItemState.LOADED;
            }
            catch (Exception)
            {
                ItemState = MediaItemState.ERROR;
            }
            finally
            {
                RWLock.ExitUpgradeableReadLock();
            }
           
        }
    }
}
