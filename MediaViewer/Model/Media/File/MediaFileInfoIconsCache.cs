using MediaViewer.Model.Media.Base.Item;
using MediaViewer.UserControls.MediaGridItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MediaViewer.Model.Media.File
{
    class MediaFileInfoIconsCache : InfoIconsCache
    {
        public MediaFileInfoIconsCache(List<BitmapImage> icons) :
            base(icons)
        {

        }

        public override System.Windows.Media.Imaging.BitmapSource getInfoIconsBitmap(MediaItem item)
        {
            BitmapSource bitmap = null;

            String key = "";

            if (item.ItemState != MediaItemState.LOADED)
            {
                return (bitmap);
            }

            if (item.Metadata != null)
            {
                if (item.Metadata.IsImported)
                {
                    key += '0';
                }

                if (!item.Metadata.SupportsXMPMetadata)
                {
                    key += '1';
                }
            }

            if (item.HasTags)
            {
                key += '2';
            }

            if (item.HasGeoTag)
            {
                key += '3';
            }

            if (item.IsReadOnly)
            {
                key += '4';
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
