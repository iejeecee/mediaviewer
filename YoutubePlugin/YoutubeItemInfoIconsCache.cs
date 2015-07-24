using MediaViewer.Model.Media.Base;
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

        public override System.Windows.Media.Imaging.BitmapSource getInfoIconsBitmap(MediaViewer.Model.Media.Base.MediaItem item)
        {
            BitmapSource bitmap = null;

            String key = "";

            if (item.Metadata == null)
            {
                return (bitmap);
            }

            if (item is YoutubeVideoItem)
            {
                YoutubeItemMetadata metadata = item.Metadata as YoutubeItemMetadata;
                
                if (metadata.Height >= 1080)
                {
                    key += "0";
                }

                if ((item as YoutubeVideoItem).IsEmbeddedOnly)
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
            
            if (String.IsNullOrEmpty(key))
            {
                return (null);
            }
            else
            {
                return (ImageHash[key]);
            }
        }
    }
}
