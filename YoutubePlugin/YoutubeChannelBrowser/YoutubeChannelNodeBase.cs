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

    class YoutubeChannelNodeBase : SharpTreeNode
    {
        protected String ImageUrl { get; set; }
       
        protected YoutubeChannelNodeBase(String imageUrl)
        {           
            ImageUrl = imageUrl;
            NrVideos = 0;
        }

        public override object Icon
        {
            get
            {
                return loadIcon();
            }
        }

        protected virtual ImageSource loadIcon()
        {                                
            BitmapFrame frame = BitmapFrame.Create(new Uri(ImageUrl, UriKind.Absolute));
            
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

        ulong? nrVideos;

        public ulong? NrVideos
        {
            get { return nrVideos; }
            set
            {
                nrVideos = value;
                RaisePropertyChanged("NrVideos");
            }
        }
    
        String name;

        public String Name
        {
            get { return name; }
            set
            {
                name = value;

                RaisePropertyChanged("Name");
                RaisePropertyChanged("Text");

            }
        }

        public override object Text
        {
            get
            {
                return (Name);
            }
        }

        protected String toolTip;
        public override object ToolTip
        {
          
            get
            {
                return toolTip;
            }
        }
    }
}
