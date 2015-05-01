using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.State.CollectionView
{
    public class MediaStateCollectionViewChangedEventArgs : MediaStateChangedEventArgsBase<SelectableMediaItem>
    {
        public MediaStateCollectionViewChangedEventArgs(MediaStateChangedAction action) :
            base(action)
        {
           
           
        }

        public MediaStateCollectionViewChangedEventArgs(MediaStateChangedAction action, SelectableMediaItem item) :
            base(action, item)
        {
            
        }

        public MediaStateCollectionViewChangedEventArgs(MediaStateChangedAction action, IEnumerable<SelectableMediaItem> items) :
            base(action, items)
        {
            
        }

        public MediaStateCollectionViewChangedEventArgs(MediaStateChangedAction action, IEnumerable<String> locations) :
            base(action, locations)
        {
                       
        }
   
        public MediaStateCollectionViewChangedEventArgs(MediaStateChangedAction action, IEnumerable<SelectableMediaItem> newItems, IEnumerable<String> oldLocations) :            
            base(action, newItems, oldLocations)
        {
            
           
        }
    }
}
