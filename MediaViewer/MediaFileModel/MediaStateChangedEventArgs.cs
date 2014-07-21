using MediaViewer.MediaFileModel.Watcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileModel
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

        public MediaStateChangedEventArgs(MediaStateChangedAction action, IEnumerable<MediaFileItem> newItems, IEnumerable<MediaFileItem> oldItems)
        {
            this.action = action;

            this.newItems = newItems;
            this.oldItems = oldItems;

        }

        MediaStateChangedAction action;

        internal MediaStateChangedAction Action
        {
            get { return action; }
            private set { action = value; }
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
