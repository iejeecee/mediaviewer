using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.Base.State
{
    public class MediaStateChangedEventArgsBase<T> : EventArgs
    {

        protected MediaStateChangedEventArgsBase(MediaStateChangedAction action)
        {
            this.action = action;

            if (action != MediaStateChangedAction.Clear)
            {
                throw new ArgumentException("Only MediaStateChangedAction.Clear can use constructor without items");
            }

            NewItems = null;
            OldItems = null;

        }

        protected MediaStateChangedEventArgsBase(MediaStateChangedAction action, T item)
        {
            this.action = action;

            if (action == MediaStateChangedAction.Add)
            {
                NewItems = new List<T>() { item };
            }
            else if (action == MediaStateChangedAction.Remove)
            {
                OldItems = new List<T>() { item };
            }
        }

        protected MediaStateChangedEventArgsBase(MediaStateChangedAction action, IEnumerable<T> items)
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

        protected MediaStateChangedEventArgsBase(MediaStateChangedAction action, IEnumerable<String> locations)
        {
            this.action = action;

            OldLocations = locations;
        }

        protected MediaStateChangedEventArgsBase(MediaStateChangedAction action, IEnumerable<T> newItems, IEnumerable<String> oldLocations)
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

        IEnumerable<T> oldItems;

        public IEnumerable<T> OldItems
        {
            get { return oldItems; }
            private set { oldItems = value; }
        }

        IEnumerable<T> newItems;

        public IEnumerable<T> NewItems
        {
            get { return newItems; }
            private set { newItems = value; }
        }
    }
}
