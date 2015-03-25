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
    public enum MediaStateChangedAction
    {
        Add,
        Remove,
        Clear,
        Modified,
        Replace   
    }

    public class MediaStateChangedEventArgs : EventArgs
    {

        public MediaStateChangedEventArgs(MediaStateChangedAction action)
        {
            this.action = action;

            if (action != MediaStateChangedAction.Clear)
            {
                throw new ArgumentException("Only MediaStateChangedAction.Clear can use constructor without items");
            }

            NewItems = null;
            OldItems = null;
           
        }

        public MediaStateChangedEventArgs(MediaStateChangedAction action, MediaItem item)
        {
            this.action = action;

            if (action == MediaStateChangedAction.Add)
            {
                NewItems = new List<MediaItem>() {item};
            }
            else if (action == MediaStateChangedAction.Remove)
            {
                OldItems = new List<MediaItem>() { item };
            }
        }
        
        public MediaStateChangedEventArgs(MediaStateChangedAction action, IEnumerable<MediaItem> items)
        {
            this.action = action;

            if (action == MediaStateChangedAction.Add)
            {
                NewItems = items;
            }
            else if (action == MediaStateChangedAction.Remove)
            {
                OldItems = items;
            }
        }

        public MediaStateChangedEventArgs(MediaStateChangedAction action, IEnumerable<String> locations)
        {
            this.action = action;
           
            OldLocations = locations;            
        }
   
        public MediaStateChangedEventArgs(MediaStateChangedAction action, IEnumerable<MediaItem> newItems, IEnumerable<String> oldLocations)
        {
            this.action = action;

            this.newItems = newItems;
            this.oldLocations = oldLocations;
           
        }

        MediaStateChangedAction action;

        public MediaStateChangedAction Action
        {
            get { return action; }
            private set { action = value; }
        }
      
        IEnumerable<String> oldLocations;

        public IEnumerable<String> OldLocations
        {
            get { return oldLocations; }
            private set { oldLocations = value; }
        }

        IEnumerable<MediaItem> oldItems;

        public IEnumerable<MediaItem> OldItems
        {
            get { return oldItems; }
            private set { oldItems = value; }
        }

        IEnumerable<MediaItem> newItems;

        public IEnumerable<MediaItem> NewItems
        {
            get { return newItems; }
            private set { newItems = value; }
        }
    }
}
