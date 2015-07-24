using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubePlugin.YoutubeChannelBrowser
{
    class YoutubeChannelVideosNode : YoutubeNodeBase
    {
        public YoutubeChannelVideosNode(String channelId) :
            base("pack://application:,,,/YoutubePlugin;component/Resources/Icons/playlist.ico")
        {
            
            ChannelId = channelId;
        }
    }
}
