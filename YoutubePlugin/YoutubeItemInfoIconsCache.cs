using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.Base.Item;
using MediaViewer.UserControls.MediaGridItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YoutubePlugin.Item;

namespace YoutubePlugin
{
    class YoutubeItemInfoIconsCache : InfoIconsCache
    {
        public YoutubeItemInfoIconsCache(List<BitmapImage> icons) :
            base(icons)
        {

        }

        protected override String getKey(MediaItem item)
        {
            String key = "";

            if (item is YoutubeVideoItem)
            {
                YoutubeVideoItem videoItem = item as YoutubeVideoItem;

                YoutubeItemMetadata metadata = item.Metadata as YoutubeItemMetadata;

                if (metadata.Height >= 2160)
                {
                    key += "4";
                }
                else if (metadata.Height >= 1080)
                {
                    key += "0";
                }

                if (videoItem.IsEmbeddedOnly || !videoItem.HasPlayableStreams)
                {
                    key += "1";
                }
            }
            else if (item is YoutubeChannelItem)
            {
                key += "2";
            }
            else if (item is YoutubePlaylistItem)
            {
                key += "3";
            }

            return key;
        }
       
        public override string getToolTip(int iconNr, MediaItem item)
        {
            String key = getKey(item);

            if (String.IsNullOrEmpty(key)) return (null);

            char icon = key[iconNr];

            String toolTip = "";

            switch (icon)
            {
                case '0':
                    {
                        toolTip = "1080p";
                        break;
                    }
                case '1':
                    {
                        toolTip = "Playback Not Allowed";
                        break;
                    }
                case '2':
                    {
                        toolTip = "Channel";
                        break;
                    }
                case '3':
                    {
                        toolTip = "Playlist";
                        break;
                    }
                case '4':
                    {
                        toolTip = "2160p";
                        break;
                    }    
                default:
                    {
                        break;
                    }

            }

            return (toolTip);
        }
    }
}
