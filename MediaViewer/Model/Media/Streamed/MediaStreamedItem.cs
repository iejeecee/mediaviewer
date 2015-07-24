using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.Streamed
{
    public class MediaStreamedItem : MediaItem
    {
        public MediaStreamedItem(String location, String name = null, MediaItemState state = MediaItemState.EMPTY) :
            base(location, name, state)
        {
           
        }

        public override void readMetadata(metadata.Metadata.MetadataFactory.ReadOptions options, System.Threading.CancellationToken token)
        {           
            ItemState = MediaItemState.LOADED;
        }

        
    }
}
