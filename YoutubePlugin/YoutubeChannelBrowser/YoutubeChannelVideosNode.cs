using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubePlugin.YoutubeChannelBrowser
{
    class YoutubeChannelVideosNode : YoutubeChannelNodeBase
    {       
        public YoutubeChannelVideosNode(String channelId) :
            base("pack://application:,,,/YoutubePlugin;component/Resources/Icons/youtube.ico")
        {
            Name = "Videos";
            ChannelId = channelId;
        }
    }
}
