using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubePlugin.YoutubeChannelBrowser
{

    class YoutubeChannelPlaylistsNode : YoutubeChannelNodeBase
    {       
        public YoutubeChannelPlaylistsNode(String channelId) :
            base("pack://application:,,,/YoutubePlugin;component/Resources/Icons/playlist.ico")
        {
            Name = "Playlists";
            ChannelId = channelId;
        }
    }
}
