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

        public MediaStateChangedEventArgs(MediaStateChangedAction action, MediaFileItem item)
        {
            this.action = action;

            if (action == MediaStateChangedAction.Add)
            {
                NewItems = new List<MediaFileItem>() {item};
            }
            else if (action == MediaStateChangedAction.Remove)
            {
                OldItems = new List<MediaFileItem>() { item };
            }
        }
        
        public MediaStateChangedEventArgs(MediaStateChangedAction action, IEnumerable<MediaFileItem> items)
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
   
        public MediaStateChangedEventArgs(MediaStateChangedAction action, IEnumerable<MediaFileItem> newItems, IEnumerable<String> oldLocations)
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

        IEnumerable<MediaFileItem> oldItems;

        public IEnumerable<MediaFileItem> OldItems
        {
            get { return oldItems; }
            private set { oldItems = value; }
        }

        IEnumerable<MediaFileItem> newItems;

        public IEnumerable<MediaFileItem> NewItems
        {
            get { return newItems; }
            private set { newItems = value; }
        }
    }
}
