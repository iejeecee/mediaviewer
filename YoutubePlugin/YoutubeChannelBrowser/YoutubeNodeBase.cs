using ICSharpCode.TreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YoutubePlugin.YoutubeChannelBrowser
{
    class YoutubeNodeBase : SharpTreeNode
    {
        protected String ImageUrl { get; set; }

        protected YoutubeNodeBase(String imageUrl)
        {
            LazyLoading = true;

            ImageUrl = imageUrl;
        }

        public override object Icon
        {
            get
            {
                return loadIcon();
            }
        }

        protected ImageSource loadIcon()
        {
            var frame = BitmapFrame.Create(new Uri(ImageUrl, UriKind.Absolute));
            return frame;
        }

        String channelId;

        public String ChannelId
        {
            get { return channelId; }
            set
            {
                channelId = value;

                RaisePropertyChanged("ChannelId");

            }
        }
    }
}
