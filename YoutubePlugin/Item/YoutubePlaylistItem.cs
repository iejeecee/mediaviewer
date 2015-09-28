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
        
        public YoutubePlaylistItem(Playlist result, int relevance)
            : base(result.Snippet.Title, result, relevance)
        {
            ChannelTitle = result.Snippet.ChannelTitle;
            ChannelId = result.Snippet.ChannelId;

            ResourceId = new ResourceId();       
            ResourceId.Kind = "youtube#playlist";
            ResourceId.PlaylistId = result.Id; 
            
            Thumbnail = result.Snippet.Thumbnails; 
        
            PublishedAt = result.Snippet.PublishedAt;
            Description = result.Snippet.Description;
            PlaylistId = result.Id;
        }


        public YoutubePlaylistItem(SearchResult result, int relevance)
            : base(result.Snippet.Title, result, relevance)
        {
            ChannelTitle = result.Snippet.ChannelTitle;
            ChannelId = result.Snippet.ChannelId;

            ResourceId = result.Id;         
            Thumbnail = result.Snippet.Thumbnails; 

            PlaylistId = result.Id.PlaylistId;  
            PublishedAt = result.Snippet.PublishedAt;
            Description = result.Snippet.Description;
        }

        

       
       
    }
}
