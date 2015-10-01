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
    public class YoutubeChannelItem : YoutubeItem
    {
        
        public YoutubeChannelItem(SearchResult item, int relevance) :
            base(item.Snippet.Title, item, relevance)
        {
            Info = item;

            ChannelTitle = item.Snippet.ChannelTitle;
            ChannelId = item.Snippet.ChannelId;

            ResourceId = item.Id;
            Thumbnail = item.Snippet.Thumbnails;
            
            PublishedAt = item.Snippet.PublishedAt;
            Description = item.Snippet.Description;
          
        }
        
    }
}
