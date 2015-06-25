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
        public SearchResult SearchResult { get; set; }
      
        public YoutubeItem(SearchResult result, int relevance) :
            base(null,result.Snippet.Title)
        {
            SearchResult = result;
            Relevance = relevance;
        }

       
        protected BitmapSource loadThumbnail(out String mimeType, CancellationToken token)
        {
            MemoryStream data = new MemoryStream();
            BitmapSource bitmapSource = null;

            try
            {
                StreamUtils.readHttpRequest(new Uri(SearchResult.Snippet.Thumbnails.High.Url), data, out mimeType, token);

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
    }
}
