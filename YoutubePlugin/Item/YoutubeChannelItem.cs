using Google.Apis.YouTube.v3.Data;
using MediaViewer.Model.Media.Base;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubePlugin.Item
{
    class YoutubeChannelItem : YoutubeItem
    {
        public YoutubeChannelItem(SearchResult result, int relevance) :
            base(result.Snippet.Title, result, relevance)
        {
            Info = result;

            ChannelTitle = result.Snippet.ChannelTitle;
            ChannelId = result.Snippet.ChannelId;

            ResourceId = result.Id;
            Thumbnail = result.Snippet.Thumbnails;
            
            PublishedAt = result.Snippet.PublishedAt;
            Description = result.Snippet.Description;
          
        }
        
    }
}
