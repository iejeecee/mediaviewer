using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using MediaViewer.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YoutubePlugin.Item;

namespace YoutubePlugin.YoutubeChannelBrowser
{
    [Serializable]
    public class YoutubeChannelNodeState
    {
        protected YoutubeChannelNodeState()
        {

        }
        
        public YoutubeChannelNodeState(YoutubeChannelItem item)
        {
            Info = item.Info as SearchResult;
      
            if (item.Metadata != null)
            {
                ThumbString = System.Convert.ToBase64String(item.Metadata.Thumbnail.ImageData);
            }
            else
            {
                ThumbString = null;
            }

            ChannelsResource.ListRequest listRequest = new ChannelsResource.ListRequest(YoutubeViewModel.Youtube, "statistics");

            listRequest.Id = item.ChannelId;

            ChannelListResponse response = listRequest.Execute();

            if (response.Items.Count > 0)
            {
                Statistics = response.Items[0].Statistics;
            }
        }

        public BitmapSource getChannelThumb()
        {            
            return (ImageUtils.jpegBase64StringToImage(ThumbString));            
        }

        public String ThumbString { get; set; }
        public SearchResult Info { get; set; }      
        public ChannelStatistics Statistics { get; set; }
    }
}
