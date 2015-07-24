using Google.Apis.Requests;
using Google.Apis.YouTube.v3.Data;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
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
    abstract class YoutubeItem : MediaStreamedItem
    {
        public int Relevance { get; protected set; }
        public IDirectResponseSchema Info { get; set; }

        
        public YoutubeItem(IDirectResponseSchema result, int relevance) :
            base(null,getTitle(result))
        {
            Info = result;
            Relevance = relevance;
        }

       
        protected BitmapSource loadThumbnail(out String mimeType, CancellationToken token)
        {
            MemoryStream data = new MemoryStream();
            BitmapSource bitmapSource = null;

            try
            {
              
                StreamUtils.readHttpRequest(new Uri(ThumbnailUrl), data, out mimeType, token);

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


        static string getTitle(IDirectResponseSchema result)
        {
            if (result is SearchResult)
            {
                return (result as SearchResult).Snippet.Title;
            }
            else if (result is PlaylistItem)
            {
                return (result as PlaylistItem).Snippet.Title;
            }

            return ("");
        }

        public String ThumbnailUrl
        {
            get
            {
                String thumbnailUrl = null;

                if (Info is SearchResult)
                {
                    thumbnailUrl = (Info as SearchResult).Snippet.Thumbnails.High.Url;
                }
                else if (Info is PlaylistItem)
                {
                    thumbnailUrl = (Info as PlaylistItem).Snippet.Thumbnails.High.Url;
                }

                return thumbnailUrl;
            }
        }

        public String ChannelTitle
        {
            get
            {
                String channelId = null;

                if (Info is SearchResult)
                {
                    channelId = (Info as SearchResult).Snippet.ChannelTitle;
                }
                else if (Info is PlaylistItem)
                {
                    channelId = (Info as PlaylistItem).Snippet.ChannelTitle;
                }

                return (channelId);
            }
        }


        public String ChannelId
        {
            get
            {
                String channelId = null;

                if (Info is SearchResult)
                {
                    channelId = (Info as SearchResult).Snippet.ChannelId;
                }
                else if (Info is PlaylistItem)
                {
                    channelId = (Info as PlaylistItem).Snippet.ChannelId;
                }

                return (channelId);
            }
        }

        ResourceId ResourceId
        {
            get
            {
                ResourceId id = null;

                if (Info is SearchResult)
                {
                    id = (Info as SearchResult).Id;
                }
                else if (Info is PlaylistItem)
                {
                    id = (Info as PlaylistItem).Snippet.ResourceId;
                }

                return (id);
            }

        }
        
    }
}
