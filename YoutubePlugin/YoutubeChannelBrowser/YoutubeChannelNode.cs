using Google.Apis.YouTube.v3.Data;
using ICSharpCode.TreeView;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YoutubePlugin.Item;

namespace YoutubePlugin.YoutubeChannelBrowser
{  
    class YoutubeChannelNode : YoutubeChannelNodeBase
    {
        public YoutubeChannelNodeState State { get; set; }
          
        public YoutubeChannelNode(YoutubeChannelNodeState state) :
            base("pack://application:,,,/YoutubePlugin;component/Resources/Icons/channel.ico")
        {
            State = state;

            Name = State.Info.Snippet.Title;
            ChannelId = State.Info.Snippet.ChannelId;          

            toolTip = State.Info.Snippet.Description;
            NrVideos = 0;

            Children.Add(new YoutubeChannelVideosNode(ChannelId));
            Children.Add(new YoutubeChannelPlaylistsNode(ChannelId));

            if (State.Statistics != null)
            {
                (Children[0] as YoutubeChannelVideosNode).NrVideos = State.Statistics.VideoCount;
            }
        }

        public void updateStatistics(ChannelStatistics statistics)
        {
            if (State.Statistics != null)
            {      
                NrVideos = statistics.VideoCount < State.Statistics.VideoCount ? 0 : statistics.VideoCount - State.Statistics.VideoCount;
            }

            State.Statistics = statistics;
            (Children[0] as YoutubeChannelVideosNode).NrVideos = statistics.VideoCount;
        }

        protected override ImageSource loadIcon()
        {
            ImageSource channelThumb = State.getChannelThumb();

            if (channelThumb == null)
            {
                return base.loadIcon();
            }
            else
            {
                return (channelThumb);
            }
        }
                          
    }
}
