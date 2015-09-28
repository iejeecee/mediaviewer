using ICSharpCode.TreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YoutubePlugin.Item;

namespace YoutubePlugin.YoutubeChannelBrowser
{
    class YoutubeChannelNode : YoutubeNodeBase
    {
        BitmapSource Thumb { get; set; }

        public YoutubeChannelNode(String name, String channelId) :
            base("pack://application:,,,/YoutubePlugin;component/Resources/Icons/channel.ico")
        {
            Name = name;
            ChannelId = channelId;
            Thumb = null;
        }

        public YoutubeChannelNode(YoutubeChannelItem channelItem) :
            base(channelItem.Thumbnail.Medium.Url)
        {
            Name = channelItem.Name;
            ChannelId = channelItem.ChannelId;
            if (channelItem.Metadata != null)
            {
                Thumb = channelItem.Metadata.Thumbnail.Image;
            }
            toolTip = channelItem.Description;
        }

        protected override ImageSource loadIcon()
        {
            if (!ImageUrl.StartsWith("http") || Thumb == null) return base.loadIcon();

            return (Thumb);
        }

        protected override void LoadChildren()
        {
            Children.Add(new YoutubeChannelVideosNode(ChannelId));
            Children.Add(new YoutubeChannelPlaylistsNode(ChannelId));           
        }
                       
    }
}
