using Google.Apis.YouTube.v3.Data;
using MediaViewer.Model.Media.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YoutubePlugin.Item
{
    class YoutubePlaylistItem : YoutubeItem
    {
        public String PlaylistId { get; protected set; }
        
        public YoutubePlaylistItem(Playlist item, int relevance)
            : base(item.Snippet.Title, item, relevance)
        {
            
            ChannelTitle = item.Snippet.ChannelTitle;
            ChannelId = item.Snippet.ChannelId;

            ResourceId = new ResourceId();       
            ResourceId.Kind = "youtube#playlist";
            ResourceId.PlaylistId = item.Id; 
            
            Thumbnail = item.Snippet.Thumbnails; 
        
            PublishedAt = item.Snippet.PublishedAt;
            Description = item.Snippet.Description;
            PlaylistId = item.Id;
        }


        public YoutubePlaylistItem(SearchResult item, int relevance)
            : base(item.Snippet.Title, item, relevance)
        {
            ChannelTitle = item.Snippet.ChannelTitle;
            ChannelId = item.Snippet.ChannelId;

            ResourceId = item.Id;         
            Thumbnail = item.Snippet.Thumbnails; 

            PlaylistId = item.Id.PlaylistId;  
            PublishedAt = item.Snippet.PublishedAt;
            Description = item.Snippet.Description;
        }

        

       
       
    }
}
