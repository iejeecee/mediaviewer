using Google.Apis.Requests;
using Google.Apis.YouTube.v3.Data;
using MediaViewer.Infrastructure.Utils;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.Base.Item;
using MediaViewer.Model.Media.Base.Metadata;
using MediaViewer.Model.Media.Streamed;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media.Imaging;

namespace YoutubePlugin.Item
{
    public abstract class YoutubeItem : MediaStreamedItem
    {
        public int Relevance { get; protected set; }
        public IDirectResponseSchema Info { get; set; }
        
        public YoutubeItem(String title, IDirectResponseSchema result, int relevance) :
            base(null, title)
        {
            Title = title;
            Info = result;
            Relevance = relevance;
        }

        public override void readMetadata_URLock(MetadataFactory.ReadOptions options, System.Threading.CancellationToken token)
        {
            String mimeType;

            try 
            {           
                ItemState = MediaItemState.LOADING;

                YoutubeItemMetadata metaData = new YoutubeItemMetadata();

                SearchResult searchInfo = Info as SearchResult;
                
                metaData.Thumbnail = new MediaViewer.MediaDatabase.Thumbnail(loadThumbnail(out mimeType, token));
                metaData.CreationDate = PublishedAt;
                metaData.Title = Name;
                metaData.Description = String.IsNullOrEmpty(Description) ? Name : Description;

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

       
        protected BitmapSource loadThumbnail(out String mimeType, CancellationToken token)
        {
            MemoryStream data = new MemoryStream();
            BitmapSource bitmapSource = null;

            try
            {
              
                StreamUtils.readHttpRequest(new Uri(Thumbnail.Medium.Url), data, out mimeType, token);

                BitmapDecoder decoder = BitmapDecoder.Create(data,
                                    BitmapCreateOptions.PreservePixelFormat,
                                    BitmapCacheOption.OnLoad);
                bitmapSource = decoder.Frames[0];

                bitmapSource.Freeze();

            }
            finally
            {
                data.Close();
            }

            return (bitmapSource);
        }
        
        protected NameValueCollection getInfo(Uri location, CancellationToken token)
        {
            String mimeType;
            MemoryStream data = new MemoryStream();
            NameValueCollection info = null;

            try
            {
                StreamUtils.readHttpRequest(location, data, out mimeType, token);

                var sr = new StreamReader(data);
                var infoResponse = sr.ReadToEnd();
                info = HttpUtility.ParseQueryString(infoResponse);
            }
            finally
            {
                data.Close();
            }

            return(info);            

        }

        public override bool Equals(MediaItem other)
        {
            YoutubeItem otherItem = (YoutubeItem)other;
           
            return (ResourceId.Equals(otherItem.ResourceId));
        }

        public String Title { get; protected set; }
        public String ChannelTitle { get; protected set; }
        public String ChannelId { get; protected set; }
        public ResourceId ResourceId { get; protected set; }
        public ThumbnailDetails Thumbnail { get; protected set; }    
        public DateTime? PublishedAt { get; protected set; }
        public String Description { get; protected set; }

        protected override void QueueOnPropertyChangedEvent(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }
}
