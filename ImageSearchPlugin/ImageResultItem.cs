using MediaViewer.Infrastructure.Utils;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.Streamed;
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

namespace ImageSearchPlugin
{
    class ImageResultItem : MediaStreamedItem
    {
        public Bing.ImageResult ImageInfo { protected set; get; }
       
        public ImageResultItem(Bing.ImageResult imageResult, int relevance) :
            base(imageResult.MediaUrl, imageResult.Title)
        {            
            ImageInfo = imageResult;
            Relevance = relevance;
        }

        public override void readMetadata_URLock(MediaViewer.Model.metadata.Metadata.MetadataFactory.ReadOptions options, System.Threading.CancellationToken token)
        {
            MemoryStream data = new MemoryStream();
            String mimeType;
            
            try
            {
                ItemState = MediaItemState.LOADING;

                StreamUtils.readHttpRequest(new Uri(ImageInfo.Thumbnail.MediaUrl), data, out mimeType, token);

                BitmapDecoder decoder = BitmapDecoder.Create(data,
                                    BitmapCreateOptions.PreservePixelFormat,
                                    BitmapCacheOption.OnLoad);
                BitmapSource bitmapSource = decoder.Frames[0];
               
                bitmapSource.Freeze();

                ImageMetadata metaData = new ImageMetadata();
                metaData.Thumbnails.Add(new Thumbnail(bitmapSource));

                metaData.Title = ImageInfo.Title;
                metaData.Location = ImageInfo.MediaUrl;
                metaData.Width = ImageInfo.Width.HasValue ? ImageInfo.Width.Value : 0;
                metaData.Height = ImageInfo.Height.HasValue ? ImageInfo.Height.Value : 0;
                metaData.SizeBytes = ImageInfo.FileSize.HasValue ? ImageInfo.FileSize.Value : 0;
                metaData.MimeType = ImageInfo.ContentType;
               
                Metadata = metaData;

                ItemState = MediaItemState.LOADED;
            }          
            catch (Exception e)
            {
                if (e is System.Net.WebException &&
                    ((System.Net.WebException)e).Status == WebExceptionStatus.Timeout)
                {
                    ItemState = MediaItemState.TIMED_OUT;
                }
                else
                {
                    ItemState = MediaItemState.ERROR;
                }
            }
                      
        }

        int relevance;

        public int Relevance
        {
            get { return relevance; }
            set { SetProperty(ref relevance, value); }
        }

        public override bool Equals(MediaItem other)
        {
            ImageResultItem item = (ImageResultItem)other;

            return (ImageInfo.Thumbnail.MediaUrl.Equals(item.ImageInfo.Thumbnail.MediaUrl));
        }

        protected override void QueueOnPropertyChangedEvent(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }
}
