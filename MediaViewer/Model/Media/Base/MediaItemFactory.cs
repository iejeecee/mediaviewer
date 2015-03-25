using MediaViewer.Model.Media.File;
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
        static MediaItem Create(String filename, MediaItemState state = MediaItemState.EMPTY)
        {
            MediaItem item = null;

            if (FileUtils.isUrl(filename))
            {

            }
            else
            {
                item = MediaFileItem.Factory.create(filename);
            }

            return (item);
        }

    }
}
