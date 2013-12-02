using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileModel.Watcher
{
    class MediaFileState : ReaderWriterLockedCollection<MediaFileItem>
    {
     
        /// <summary>
        /// Is fired everytime the state is changed
        /// This event fires on a background thread, doing changes to the ui in
        /// the eventhandler need to take this into account
        /// A write lock will be held on the state during this event
        /// </summary>
        public event NotifyCollectionChangedEventHandler StateChangedLocked;
        /// <summary>
        /// Is fired everytime the collection of selected items changes
        /// Note that this event can be fired on a background thread,
        /// doing changes to the ui in the eventhandler need to take this into account
        /// </summary>
        public event EventHandler ItemIsSelectedChanged;

        /// <summary>
        /// Returns true when all items are added
        /// Returns false when one or more items already exist in the list
        /// no items will actually be added in this case
        /// </summary>
        /// <param name="newItems"></param>
        /// <returns></returns>
        override public bool AddRange(IEnumerable<MediaFileItem> newItems) 
        {
            rwLock.EnterWriteLock();
          
            try
            {
                if (Contains(newItems) == true)
                {
                    return (false);
                }

                bool itemIsSelectedChanged = false;
                int startingIndex = Count;

                items.AddRange(newItems);

                foreach (MediaFileItem item in newItems)
                {
                    item.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Item_PropertyChanged);
                    if (item.IsSelected == true)
                    {
                        itemIsSelectedChanged = true;
                    }
                }

                NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(
                   NotifyCollectionChangedAction.Add, items.ToList(), startingIndex);

                OnCollectionChangedEventLocked(args);

                if (itemIsSelectedChanged)
                {
                    OnItemIsSelectedChanged();
                }

                return (true);
            }
            finally
            {                              
                rwLock.ExitWriteLock();
            }

        }

        override public void Clear()
        {
            rwLock.EnterWriteLock();
           
            try
            {

                bool itemIsSelectedChanged = false;
                List<MediaFileItem> removed = new List<MediaFileItem>(Items);

                foreach (MediaFileItem item in Items)
                {
                    item.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(Item_PropertyChanged);

                    if (item.IsSelected == true)
                    {
                        itemIsSelectedChanged = true;
                    }
                }

                items.Clear();

                NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(
                  NotifyCollectionChangedAction.Remove, removed);

                OnCollectionChangedEventLocked(args);

                if (itemIsSelectedChanged)
                {
                    OnItemIsSelectedChanged();
                }
            }
            finally
            {
               
                rwLock.ExitWriteLock();
            }

        }

        override public void RemoveAll(IEnumerable<MediaFileItem> removeItems)
        {

            rwLock.EnterWriteLock();          
            try
            {

                bool itemIsSelectedChanged = false;
                List<MediaFileItem> removed = new List<MediaFileItem>();

                foreach (MediaFileItem removeItem in removeItems)
                {
                    foreach (MediaFileItem item in items)
                    {
                        if (removeItem.Equals(item))
                        {
                            item.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(Item_PropertyChanged);
                            if (item.IsSelected == true)
                            {
                                itemIsSelectedChanged = true;
                            }

                            removed.Add(item);
                            items.Remove(item);
                            break;
                        }
                        
                    }
                    
                }
              
                NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, removed);

                OnCollectionChangedEventLocked(args);

                if (itemIsSelectedChanged)
                {
                    OnItemIsSelectedChanged();
                }
            }
            finally
            {                
                rwLock.ExitWriteLock();
            }
        }

        override public void ReplaceAll(IEnumerable<MediaFileItem> oldItems, IEnumerable<MediaFileItem> newItems)
        {

            rwLock.EnterWriteLock();
            try
            {
           
                bool itemIsSelectedChanged = false;
                List<MediaFileItem> removed = new List<MediaFileItem>();
                List<MediaFileItem> added = new List<MediaFileItem>();

                int nrOldItems = oldItems.Count();
                int nrNewItems = newItems.Count();

                int iterations = Math.Max(nrOldItems, nrNewItems);

                for (int i = 0; i < iterations; i++)
                {
                    if (i < nrOldItems)
                    {
                        MediaFileItem oldItem = Find(oldItems.ElementAt(i));
                        if (oldItem != null)
                        {
                            oldItem.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(Item_PropertyChanged);
                            if (oldItem.IsSelected == true)
                            {
                                itemIsSelectedChanged = true;
                            }

                            removed.Add(oldItem);

                            items.Remove(oldItem);
                        }

                    }

                    if (i < nrNewItems)
                    {
                        MediaFileItem newItem = Find(newItems.ElementAt(i));
                        if (newItem == null)
                        {
                            newItem = newItems.ElementAt(i);

                            newItem.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Item_PropertyChanged);
                            if (newItem.IsSelected == true)
                            {
                                itemIsSelectedChanged = true;
                            }

                            items.Add(newItem);
                            added.Add(newItem);
                        }
                    }              
                }

                if (removed.Count > 0 || added.Count > 0)
                {
                    NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, removed, added);

                    OnCollectionChangedEventLocked(args);
                }

                if (itemIsSelectedChanged)
                {
                    OnItemIsSelectedChanged();
                }
            
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public void SelectAllItems()
        {
            rwLock.EnterReadLock();
            try
            {
                foreach (MediaFileItem item in items)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                    item.IsSelected = true;
                    item.PropertyChanged += Item_PropertyChanged;
                }

                OnItemIsSelectedChanged();
               
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        public void DeselectAllItems()
        {
            rwLock.EnterReadLock();
            try
            {
                foreach (MediaFileItem item in items)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                    item.IsSelected = false;
                    item.PropertyChanged += Item_PropertyChanged;
                }

                OnItemIsSelectedChanged();
               
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }


        public List<MediaFileItem> GetSelectedItems()
        {
            List<MediaFileItem> selected = new List<MediaFileItem>();

            rwLock.EnterReadLock();

            try
            {
                foreach (MediaFileItem item in items)
                {
                    if (item.IsSelected)
                    {
                        selected.Add(item);
                    }
                }

                return (selected);
            }
            finally
            {
                rwLock.ExitReadLock();
            }           
        }

        void Item_PropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsSelected"))
            {
                OnItemIsSelectedChanged();
            }
        }

        void OnCollectionChangedEventLocked(NotifyCollectionChangedEventArgs args)
        {
            if (StateChangedLocked != null)
            {
                StateChangedLocked(this, args);
            }
        }

        void OnItemIsSelectedChanged()
        {
            if (ItemIsSelectedChanged != null)
            {
                ItemIsSelectedChanged(this, EventArgs.Empty);
            }
        }

        
    }
}
