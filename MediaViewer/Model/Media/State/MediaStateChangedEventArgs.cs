using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.State
{


    public class MediaStateChangedEventArgs : MediaStateChangedEventArgsBase<MediaItem>
    {

        public MediaStateChangedEventArgs(MediaStateChangedAction action) :
            base(action)
        {
           
           
        }

        public MediaStateChangedEventArgs(MediaStateChangedAction action, MediaItem item) :
            base(action, item)
        {
            
        }

        public MediaStateChangedEventArgs(MediaStateChangedAction action, IEnumerable<MediaItem> items) :
            base(action, items)
        {
            
        }

        public MediaStateChangedEventArgs(MediaStateChangedAction action, IEnumerable<String> locations) :
            base(action, locations)
        {
                       
        }
   
        public MediaStateChangedEventArgs(MediaStateChangedAction action, IEnumerable<MediaItem> newItems, IEnumerable<String> oldLocations) :            
            base(action, newItems, oldLocations)
        {
            
           
        }

        
    }
}
