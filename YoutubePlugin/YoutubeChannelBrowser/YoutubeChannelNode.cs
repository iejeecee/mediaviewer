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
        public YoutubeChannelItem ChannelItem { get; set; }

        BitmapSource Thumb { get; set; }
       
        public YoutubeChannelNode(YoutubeChannelItem channelItem) :
            base(channelItem.Thumbnail.Medium.Url)
        {
            ChannelItem = channelItem;

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
            if (!ImageUrl.StartsWith("http"))
            {
                return base.loadIcon();
            } 
            else if (Thumb != null)
            {
                return Thumb;
            }

            MemoryStream data = new MemoryStream();      
            String mimeType;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            try
            {
                StreamUtils.readHttpRequest(new Uri(ImageUrl), data, out mimeType, tokenSource.Token);

                BitmapDecoder decoder = BitmapDecoder.Create(data,
                                    BitmapCreateOptions.PreservePixelFormat,
                                    BitmapCacheOption.OnLoad);
                Thumb = decoder.Frames[0];

                Thumb.Freeze();

            }
            finally
            {
                data.Close();
            }

            return (Thumb);
        }

        protected override void LoadChildren()
        {
            Children.Add(new YoutubeChannelVideosNode(ChannelId));
            Children.Add(new YoutubeChannelPlaylistsNode(ChannelId));           
        }
                       
    }
}
