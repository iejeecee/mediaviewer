using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.Streamed;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.Base
{
    class MediaItemFactory
    {
        public static MediaItem create(String location)
        {
            MediaItem item = null;

            if (FileUtils.isUrl(location))
            {
                item = new MediaStreamedItem(location);
            }
            else
            {
                item = MediaFileItem.Factory.create(location);
            }

            return (item);
        }

    }
}
