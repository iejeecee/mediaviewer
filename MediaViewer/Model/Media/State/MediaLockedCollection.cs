using MediaViewer.Model.Media.File;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.State
{

    /// <summary>
    /// Concurrent Collection that supports multiple readers and a single writer
    /// Also makes sure the items in the collection are unique
    /// </summary>
    /// <typeparam name="MediaFileItem"></typeparam>
    public class MediaLockedCollection : ObservableObject, IDisposable, IEnumerable<MediaFileItem>
    {

        MediaFileItemLoader itemLoader;
        
        public event EventHandler CollectionModified;

        // items in the current User Interface
        List<MediaFileItem> items;
     
        public ReadOnlyCollection<MediaFileItem> Items
        {
            get { return items.AsReadOnly(); }

        }

        protected ReaderWriterLockSlim rwLock;

        public MediaLockedCollection(bool autoLoadItems = false)
        {
            rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            items = new List<MediaFileItem>();
           
            if (autoLoadItems == true)
            {
                itemLoader = new MediaFileItemLoader();
                Task.Factory.StartNew(() => itemLoader.loadLoop());

                itemLoader.ItemFinishedLoading += itemLoader_ItemFinishedLoading;
            }

            this.autoLoadItems = autoLoadItems;
            IsLoading = false;
            
        }

        void itemLoader_ItemFinishedLoading(object sender, EventArgs e)
        {
            rwLock.EnterWriteLock();
            try
            {
                // check to make sure the loaded item is actually in the current state
                if(Items.Contains(sender as MediaFileItem)) {
                    NrLoadedItems++;

                    if (NrLoadedItems == Items.Count)
                    {
                        IsLoading = false;
                    }
                    else
                    {
                        IsLoading = true;
                    }
                }                             
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
           
        }

        bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set { isLoading = value;
            NotifyPropertyChanged();
            }
        }

        int nrLoadedItems;

        public int NrLoadedItems
        {
            get { return nrLoadedItems; }
            protected set { nrLoadedItems = value;
            NotifyPropertyChanged();
            }
        }

        
        bool autoLoadItems;

        public bool AutoLoadItems
        {
            get { return autoLoadItems; }           
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool safe)
        {
            if (safe)
            {
                if (rwLock != null)
                {
                    rwLock.Dispose();
                    rwLock = null;
                }
            }
        }

        public void EnterReaderLock()
        {
            rwLock.EnterReadLock();
        }

        public void ExitReaderLock()
        {
            rwLock.ExitReadLock();
        }

        public void EnterWriteLock()
        {
            rwLock.EnterWriteLock();
        }

        public void ExitWriteLock()
        {
            rwLock.ExitWriteLock();
        }

        public int Count
        {
            get
            {
                rwLock.EnterReadLock();
                try
                {
                    return (items.Count);
                }
                finally
                {
                    rwLock.ExitReadLock();
                }
            }
        }

        virtual public bool Add(MediaFileItem newItem)
        {
            bool isModified = false;

            rwLock.EnterWriteLock();           
            try
            {
                if (Contains(newItem) == true)
                {                
                    return (isModified = false);
                }

                items.Add(newItem);

                if (AutoLoadItems)
                {
                    if (newItem.ItemState == MediaFileItemState.LOADED)
                    {
                        NrLoadedItems++;
                    }
                    else
                    {
                        itemLoader.add(newItem);
                    }
                }

                return (isModified = true);

            }
            finally
            {
                rwLock.ExitWriteLock();

                if (isModified == true)
                {
                    OnCollectionModified();
                }

            }
           
        }
        /// <summary>
        /// Returns true when all items are added.
        /// Returns false when one or more newItems already exist in the list,
        /// none of the newItems will be added in this case
        /// </summary>
        /// <param name="newItems"></param>
        /// <returns></returns>
        virtual public bool AddRange(IEnumerable<MediaFileItem> newItems)
        {
            rwLock.EnterWriteLock();

            try {

                bool newItemsAreUnique = true;

                foreach (MediaFileItem item in newItems)
                {
                    if (Add(item) == false)
                    {
                        newItemsAreUnique = false;
                    }
                }
               
                return (newItemsAreUnique);
                        
            }
            finally
            {
                rwLock.ExitWriteLock();
               
            }

        }
        /// <summary>
        /// Remove all elements from the collection
        /// </summary>
        virtual public void Clear()
        {
            rwLock.EnterWriteLock();

            try
            {
                items.Clear();

                if (AutoLoadItems)
                {
                    NrLoadedItems = 0;
                    itemLoader.clear();
                }
            }
            finally
            {
                rwLock.ExitWriteLock();               
                OnCollectionModified();
            }

        }

        virtual public void Remove(MediaFileItem removeItem)
        {
            bool isModified = false;

            rwLock.EnterWriteLock();
            try
            {             
                foreach (MediaFileItem item in items)
                {
                    if (removeItem.Equals(item))
                    {
                        items.Remove(item);
                        isModified = true;

                        if (AutoLoadItems)
                        {
                            if (item.ItemState != MediaFileItemState.LOADING)
                            {
                                NrLoadedItems--;
                            }
                            itemLoader.remove(item);
                        }
                        break;
                    }

                }
                
            }
            finally
            {
                rwLock.ExitWriteLock();

                if (isModified == true)
                {
                    OnCollectionModified();
                }
            }
        }

        /// <summary>
        /// Remove all elements contained in removeItems from the collection.   
        /// Returns the actually removed items
        /// </summary>
        /// <param name="removeItems"></param>
        virtual public List<MediaFileItem> RemoveAll(IEnumerable<MediaFileItem> removeItems)
        {
            List<MediaFileItem> removed = new List<MediaFileItem>();

            rwLock.EnterWriteLock();
            try
            {                     
                foreach (MediaFileItem removeItem in removeItems)
                {
                    foreach (MediaFileItem item in items)
                    {
                        if (removeItem.Equals(item))
                        {                            
                            items.Remove(item);                            
                            removed.Add(item);

                            if (AutoLoadItems)
                            {
                                if (item.ItemState != MediaFileItemState.LOADING)
                                {
                                    NrLoadedItems--;
                                }
                                itemLoader.remove(item);
                            }
                            break;
                        }

                    }

                }

                return (removed);
            }
            finally
            {               
                rwLock.ExitWriteLock();

                if (removed.Count > 0)
                {
                    OnCollectionModified();
                }
            }
        }
        
        public virtual MediaFileItem Find(String location)
        {
            rwLock.EnterReadLock();
            try
            {
                foreach (MediaFileItem item in items)
                {
                    if (item.Location.Equals(location))
                    {
                        return (item);
                    }
                }

                return null;
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        public virtual bool Contains(MediaFileItem compareItem)
        {
            rwLock.EnterReadLock();

            try
            {
                foreach (MediaFileItem item in items)
                {
                    if (item.Equals(compareItem))
                    {
                        return (true);
                    }

                }

                return (false);
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        public virtual bool Contains(IEnumerable<MediaFileItem> compareItems)
        {
            rwLock.EnterWriteLock();
            try
            {
                foreach (MediaFileItem compareItem in compareItems)
                {
                    foreach (MediaFileItem item in items)
                    {
                        if (compareItem.Equals(item))
                        {
                            return (true);                            
                        }

                    }

                }

                return (false);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }


        public bool RenameRange(IEnumerable<MediaFileItem> oldItems, IEnumerable<String> newLocations)
        {
           
            rwLock.EnterWriteLock();
            try
            {
                bool success = true;

                int nrOldItems = oldItems.Count();
                int nrNewItems = newLocations.Count();

                Debug.Assert(nrOldItems == nrNewItems);
              
                for (int i = 0; i < nrOldItems; i++)
                {                   
                    MediaFileItem oldItem = Find(oldItems.ElementAt(i).Location);
                    if (oldItem == null)
                    {
                        success = false;
                        continue;
                    }

                    oldItem.Location = newLocations.ElementAt(i);                       
                }

                items.Sort();
                return (success);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        private void OnCollectionModified()
        {
            if (CollectionModified != null)
            {
                CollectionModified(this, EventArgs.Empty);
            }
        }

        public IEnumerator<MediaFileItem> GetEnumerator()
        {
            return(items.GetEnumerator());
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (items.GetEnumerator());
        }
    }
}

